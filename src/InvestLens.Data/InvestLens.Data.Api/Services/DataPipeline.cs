using InvestLens.Abstraction.Redis.Services;
using InvestLens.Data.Api.Converter;
using InvestLens.Data.Entities;
using InvestLens.Shared.Exceptions;
using System.Collections.Concurrent;
using System.Text.Json;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services;

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
        int maxConcurrentDownloads=3,
        int saveBatchSize=10_000)
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

    public async Task<int> ProcessAllDataAsync(Func<Exception, Task> failBack)
    {
        var downloadSemaphore = new SemaphoreSlim(_maxConcurrentDownloads);
        var saveQueue = new BlockingCollection<List<TEntity>>(10); // Буфер на 10 пачек

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

    private readonly ConcurrentQueue<TEntity> _accumulator = new ConcurrentQueue<TEntity>();

    private List<TEntity>? AccumulateForSaving(List<TEntity> batch, int threshold)
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
            var toSave = new List<TEntity>();
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

    protected virtual string GetKeyName => "id";

    private async Task ImportInBatches(BlockingCollection<List<TEntity>> queue, Func<Exception, Task> failBack)
    {
        var totalSaved = 0;
        var batchId = 0;

        _logger.LogInformation("Запустилась задача для сохранения данных.");

        foreach (var batch in queue.GetConsumingEnumerable())
        {
            var savedCount = await _dataWriterService.SaveDataAsync(GetKeyName, batch, batchId, failBack);
            _logger.LogInformation($"Сохранено {batch.Count} записей.");
            totalSaved += savedCount;
            batchId++;
            await _statusService.SetSaving(_correlationIdService.GetOrCreateCorrelationId(nameof(DataPipeline<TEntity, TResponse>)), totalSaved);
        }

        // Сохраняем остатки
        if (_accumulator.Any())
        {
            var savedCount = await _dataWriterService.SaveDataAsync(GetKeyName, _accumulator, batchId, failBack);
            _logger.LogInformation($"Сохранены оставшиеся {savedCount} записей.");
            totalSaved += savedCount;
            await _statusService.SetSaving(
                _correlationIdService.GetOrCreateCorrelationId(nameof(DataPipeline<TEntity, TResponse>)),
                totalSaved);
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

            return securitiesResponse.Section.Metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    protected abstract string GetUrl(params int[] args);

    private async Task<List<TEntity>> DownloadBatchAsync(int page, int batchSize)
    {
        string url = GetUrl(page, batchSize);

        if (string.IsNullOrEmpty(url)) return [];

        try
        {
            var responseMessage = await _httpClient.GetAsync(url);
            responseMessage.EnsureSuccessStatusCode();

            var json = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<TResponse>(json);

            if (response is null)
            {
                throw new MoexApiException($"Не пришли данные.");
            }

            var entities = ResponseToEntityConverters.ResponseToEntityConverter(response, page, batchSize);
            return entities.Cast<TEntity>().ToList();
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Ошибка при загрузке страницы {page}: {ex.Message}");
            return [];
        }
    }

    #endregion Privare Methods
}

public abstract class IndexDataPipeline<TEntity, TResponse> : DataPipeline<TEntity, TResponse>
    where TEntity : DictionaryBaseEntity
    where TResponse : IBaseDictionaryResponse
{
    protected IndexDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<DataPipeline<TEntity, TResponse>> logger,
        int maxConcurrentDownloads = 3, int saveBatchSize = 10000) : base(httpClient, dataWriterService, statusService,
        correlationIdService, logger, maxConcurrentDownloads, saveBatchSize)

    {
    }

    protected override string GetUrl(params int[] args)
    {
        return args[0] != 0 ? string.Empty : $"/iss/index.json";
    }
}

public class SecurityDataPipeline : DataPipeline<SecurityEntity, SecuritiesResponse>, ISecurityDataPipeline
{
    public SecurityDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<DataPipeline<SecurityEntity, SecuritiesResponse>> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger)

    {
    }

    public override string Info => "Список ценных бумаг";
    protected override string GetKeyName => "secid";

    protected override string GetUrl(params int[] args)
    {
        var page = args[0];
        var batchSize = args[1];

        return $"/iss/securities.json?start={page * batchSize}&limit={batchSize}";
    }
}

public class EngineDataPipeline : IndexDataPipeline<EngineEntity, EngineDictionaryDataResponse>, IEngineDataPipeline
{
    public EngineDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<EngineDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Список доступных торговых систем";
}

public class MarketDataPipeline : IndexDataPipeline<MarketEntity, MarketDictionaryDataResponse>, IMarketDataPipeline
{
    public MarketDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<MarketDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Справочник доступных рынков";
}

public class BoardDataPipeline : IndexDataPipeline<BoardEntity, BoardDictionaryDataResponse>, IBoardDataPipeline
{
    public BoardDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<BoardDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Справочник режимов торгов";
}

public class BoardGroupDataPipeline : IndexDataPipeline<BoardGroupEntity, BoardGroupDictionaryDataResponse>, IBoardGroupDataPipeline
{
    public BoardGroupDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<BoardGroupDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Справочник групп режимов торгов";
}

public class DurationDataPipeline : IndexDataPipeline<DurationEntity, DurationDictionaryDataResponse>, IDurationDataPipeline
{
    public DurationDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<DurationDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Справочник доступных расчетных интервалов свечей в формате HLOCV";

    protected override string GetKeyName => "interval";
}

public class SecurityTypeDataPipeline : IndexDataPipeline<SecurityTypeEntity, SecurityTypeDictionaryDataResponse>, ISecurityTypeDataPipeline
{
    public SecurityTypeDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<SecurityTypeDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Типы инструментов для торговой системы";
}

public class SecurityGroupDataPipeline : IndexDataPipeline<SecurityGroupEntity, SecurityGroupDictionaryDataResponse>, ISecurityGroupDataPipeline
{
    public SecurityGroupDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<SecurityGroupDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Группы инструментов для торговой системы";
}

public class SecurityCollectionDataPipeline : IndexDataPipeline<SecurityCollectionEntity, SecurityCollectionDictionaryDataResponse>, ISecurityCollectionDataPipeline
{
    public SecurityCollectionDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<SecurityCollectionDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Коллекции инструментов для торговой системы";
}