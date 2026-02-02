using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Converter;
using InvestLens.Data.Entities;
using InvestLens.Data.Shared.Responses;
using InvestLens.Shared.Exceptions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace InvestLens.Data.Api.Services;

public class DataPipeline : IDataPipeline
{
    private readonly HttpClient _httpClient;
    private readonly IDataService _dataService;
    private readonly ISecuritiesRefreshStatusService _statusService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<DataPipeline> _logger;
    private readonly int _maxConcurrentDownloads = 3;
    private readonly int _batchSize = 100;
    private readonly int _saveBatchSize = 10_000; // Сохраняем пачками по 10к

    public DataPipeline(HttpClient httpClient, IDataService dataService,
        ISecuritiesRefreshStatusService statusService,
        ICorrelationIdService correlationIdService,
        ILogger<DataPipeline> logger)
    {
        _httpClient = httpClient;
        _dataService = dataService;
        _statusService = statusService;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<int> ProcessAllDataAsync(Func<Exception, Task> failBack)
    {
        var downloadSemaphore = new SemaphoreSlim(_maxConcurrentDownloads);
        var saveQueue = new BlockingCollection<List<Security>>(10); // Буфер на 10 пачек

        // Запускаем задачу сохранения в отдельном потоке
        var saveTask = Task.Run(() => ImportInBatches(saveQueue, failBack))
            .ContinueWith(t => _logger.LogWarning($"SaveTask {t.Id} завершен."));

        // Запускаем задачи загрузки
        var downloadTasks = new List<Task>();

        int totalRecords = 0;
        int page = 0;
        bool breakFlag = false;

        while (true)
        {
            await downloadSemaphore.WaitAsync();

            if (breakFlag) break;

            var pageNumber = page;
            var task = Task.Run(async () =>
            {
                try
                {
                    var batch = await DownloadBatchAsync(pageNumber, _batchSize);

                    if (batch.Any())
                    {
                        Interlocked.Add(ref totalRecords, batch.Count);
                        await _statusService.SetDownloading(_correlationIdService.GetOrCreateCorrelationId(nameof(DataPipeline)), totalRecords);
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
                        saveQueue.Add(accumulated);
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            });

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

    private readonly ConcurrentQueue<Security> _accumulator = new ConcurrentQueue<Security>();

    private List<Security>? AccumulateForSaving(List<Security> batch, int threshold)
    {
        // Добавляем без блокировок
        foreach (var item in batch)
        {
            _accumulator.Enqueue(item);
        }

        // Проверяем количество
        if (_accumulator.Count >= threshold)
        {
            // Извлекаем пачку
            var toSave = new List<Security>();
            int count = 0;

            while (count < threshold && _accumulator.TryDequeue(out var item))
            {
                toSave.Add(item);
                count++;
            }

            return toSave;
        }

        return null;
    }

    private async Task ImportInBatches(BlockingCollection<List<Security>> queue, Func<Exception, Task> failBack)
    {
        var totalSaved = 0;
        var batchId = 0;

        _logger.LogInformation("Запустилась задача для сохранения данных.");

        foreach (var batch in queue.GetConsumingEnumerable())
        {
            var savedCount = await _dataService.SaveDataAsync<Security, Guid>(batch, batchId, failBack);
            _logger.LogInformation($"Сохранено {batch.Count} записей.");
            totalSaved += savedCount;
            batchId++;
            await _statusService.SetSaving(_correlationIdService.GetOrCreateCorrelationId(nameof(DataPipeline)), totalSaved);
        }

        // Сохраняем остатки
        if (_accumulator.Any())
        {
            var savedCount = await _dataService.SaveDataAsync<Security, Guid>(_accumulator, batchId, failBack);
            _logger.LogInformation($"Сохранены оставшиеся {savedCount} записей.");
            totalSaved += savedCount;
            await _statusService.SetSaving(_correlationIdService.GetOrCreateCorrelationId(nameof(DataPipeline)), totalSaved);
        }

        _logger.LogInformation("Завершилась задача для сохранения данных.");
    }

    private async Task<Dictionary<string, ColumnMetadata>> DownloadMetadataAsync()
    {
        string url = $"/iss/securities.json?iss.data=off";
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var securitiesResponse = JsonSerializer.Deserialize<SecuritiesResponse>(json);

            if (securitiesResponse is null)
            {
                throw new MoexApiException("Метаданные не пришли с MOEX.");
            }

            return securitiesResponse.Securities.Metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task<List<Security>> DownloadBatchAsync(int page, int batchSize)
    {
        string url = $"/iss/securities.json?start={page * batchSize}&limit={batchSize}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var securitiesResponse = JsonSerializer.Deserialize<SecuritiesResponse>(json);

            if (securitiesResponse is null)
            {
                throw new MoexApiException($"Не пришли данные.");
            }

            var securities = ResponseToEntityConverters.SecurityResponseToEntityConverter(securitiesResponse, page, batchSize);
            return securities;
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Ошибка при загрузке страницы {page}: {ex.Message}");
            return new List<Security>();
        }
    }

    #endregion Privare Methods
}