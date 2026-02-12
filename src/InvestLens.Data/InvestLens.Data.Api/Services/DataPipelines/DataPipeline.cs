using InvestLens.Data.Api.Converter;
using InvestLens.Data.Entities;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Exceptions;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;
using System.Collections.Concurrent;
using System.Text.Json;

namespace InvestLens.Data.Api.Services.DataPipelines;

public abstract class DataPipeline<TEntity, TResponse> : IDataPipeline
    where TEntity : BaseEntity
    where TResponse : IBaseResponse
{
    private readonly HttpClient _httpClient;
    private readonly IDataWriterService _dataWriterService;
    private readonly IRefreshStatusService _statusService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<DataPipeline<TEntity, TResponse>> _logger;
    private readonly int _maxConcurrentDownloads;
    private readonly int _batchSize = 100;
    private readonly int _saveBatchSize;

    protected DataPipeline(HttpClient httpClient, IDataWriterService dataWriterService,
        IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService,
        ILogger<DataPipeline<TEntity, TResponse>> logger,
        int maxConcurrentDownloads = 3,
        int saveBatchSize = 10_000)
    {
        _httpClient = httpClient;
        _dataWriterService = dataWriterService;
        _statusService = statusService;
        _correlationIdService = correlationIdService;
        _maxConcurrentDownloads = maxConcurrentDownloads;
        _saveBatchSize = saveBatchSize;
        _logger = logger;
    }

    public abstract string Info { get; }

    public async Task<int> ProcessAllDataAsync(Func<Exception, Task> failBack, CancellationToken cancellationToken = default)
    {
        var downloadSemaphore = new SemaphoreSlim(_maxConcurrentDownloads);
        var saveQueue = new BlockingCollection<List<TEntity>>(10); // Буфер на 10 пачек

        // Запускаем задачу сохранения в отдельном потоке
        Task saveTask;
        try
        {
            saveTask = Task.Run(() => ImportInBatches(saveQueue, failBack, cancellationToken), cancellationToken);
            _logger.LogInformation("SaveTask {TaskId} completed.", saveTask.Id);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "SaveTask canceled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveTask failed.");
            throw;
        }

        // Запускаем задачи загрузки
        var downloadTasks = new List<Task>();

        var totalRecords = 0;
        var page = 0;
        var breakFlag = false;

        while (!breakFlag && !cancellationToken.IsCancellationRequested)
        {
            await downloadSemaphore.WaitAsync(cancellationToken);

            var pageNumber = page;
            var task = Task.Run(async () =>
            {
                try
                {
                    var batch = await DownloadBatchAsync(pageNumber, _batchSize, cancellationToken);

                    if (batch.Any())
                    {
                        Interlocked.Add(ref totalRecords, batch.Count);
                        await _statusService.SetDownloading(
                            _correlationIdService.GetOrCreateCorrelationId(
                                nameof(DataPipeline<TEntity, TResponse>)), totalRecords);
                    }
                    else
                    {
                        breakFlag = true;
                        return;
                    }

                    // Накопительная буферизация
                    var accumulated = AccumulateForSaving(batch, _saveBatchSize);
                    if (accumulated != null)
                    {
                        saveQueue.Add(accumulated, cancellationToken);
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }, cancellationToken);

            downloadTasks.Add(task);

            page++;
        }

        // Ждем завершения всех загрузок
        await Task.WhenAll(downloadTasks);

        // Сигнализируем, что загрузка завершена
        saveQueue.CompleteAdding();

        // Ждем завершения сохранения
        await saveTask;

        return totalRecords;
    }

    #region Privare Methods

    private readonly ConcurrentQueue<TEntity> _accumulator = new ConcurrentQueue<TEntity>();

    private List<TEntity>? AccumulateForSaving(List<TEntity> batch, int threshold)
    {
        // Добавляем без блокировок
        foreach (var item in batch)
        {
            _accumulator.Enqueue(item);
        }

        // Проверяем количество
        if (_accumulator.Count < threshold) return null;

        // Извлекаем пачку
        var toSave = new List<TEntity>();
        int count = 0;

        while (count < threshold && _accumulator.TryDequeue(out var item))
        {
            toSave.Add(item);
            count++;
        }

        return toSave;
    }

    protected virtual string GetKeyName => "id";

    private async Task ImportInBatches(BlockingCollection<List<TEntity>> queue, Func<Exception, Task> failBack,
        CancellationToken cancellationToken)
    {
        var totalSaved = 0;
        var batchId = 0;

        _logger.LogInformation("Data saving task started.");

        foreach (var batch in queue.GetConsumingEnumerable())
        {
            var savedCount = await _dataWriterService.SaveDataAsync(GetKeyName, batch, batchId, failBack, cancellationToken);
            _logger.LogInformation($"Saved {batch.Count} records.");
            totalSaved += savedCount;
            batchId++;
            await _statusService.SetSaving(_correlationIdService.GetOrCreateCorrelationId(nameof(DataPipeline<TEntity, TResponse>)), totalSaved);
        }

        // Сохраняем остатки
        if (_accumulator.Any())
        {
            var savedCount = await _dataWriterService.SaveDataAsync(GetKeyName, _accumulator, batchId, failBack, cancellationToken);
            _logger.LogInformation($"Saved remaining {savedCount} records.");
            totalSaved += savedCount;
            await _statusService.SetSaving(
                _correlationIdService.GetOrCreateCorrelationId(nameof(DataPipeline<TEntity, TResponse>)),
                totalSaved);
        }

        _logger.LogInformation("Data saving task completed.");
    }

    protected abstract string GetUrl(params int[] args);

    private async Task<List<TEntity>> DownloadBatchAsync(int page, int batchSize, CancellationToken cancellationToken)
    {
        string url = GetUrl(page, batchSize);

        if (string.IsNullOrEmpty(url)) return [];

        try
        {
            var responseMessage = await _httpClient.GetAsync(url, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();

            var json = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var response = JsonSerializer.Deserialize<TResponse>(json);

            if (response is null)
            {
                throw new MoexApiException("The data was not received.");
            }

            var entities = ResponseToEntityConverters.ResponseToEntityConverter(response, page, batchSize);
            return entities.Cast<TEntity>().ToList();
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    #endregion Privare Methods
}