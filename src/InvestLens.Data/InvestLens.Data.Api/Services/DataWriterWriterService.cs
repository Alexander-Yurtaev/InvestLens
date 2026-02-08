using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using InvestLens.Data.Entities;
using Npgsql;
using NpgsqlTypes;
using System.Reflection;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services;

public class DataWriterWriterService : IDataWriterService
{
    private readonly string _connectionString;
    private readonly ILogger<DataWriterWriterService> _logger;

    public DataWriterWriterService(
        IConfiguration configuration,
        ILogger<DataWriterWriterService> logger)
    {
        _connectionString = ConnectionStringHelper.GetTargetConnectionString(configuration);
        _logger = logger;
    }

    public async Task<int> SaveDataAsync<TEntity>(string keyName, IEnumerable<TEntity> batch, int batchId,
        Func<Exception, Task>? failBack) where TEntity : BaseEntity
    {
        var tableName = typeof(TEntity).Name.ToLowerInvariant();
        var tempTableName = $"temp_{tableName}_batch_{batchId}";
        var columns = typeof(TEntity)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .Select(p => p.GetCustomAttribute<ColumnAttribute>()!.Name)
            .Where(name => string.Equals(keyName, "id", StringComparison.OrdinalIgnoreCase) ||
                           (!string.Equals(keyName, "id", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "id", StringComparison.OrdinalIgnoreCase)))
            .Select(name =>
                string.Equals(name, "group", StringComparison.OrdinalIgnoreCase) ? $"\"{name}\"" : name)
            .ToList();

        var selectColumns = string.Join(',', columns);
        var conflictColumns = columns.Select(name => $"{name}=EXCLUDED.{name}");

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // 1. Создаем временную таблицу
            await connection.ExecuteAsync($@"
                CREATE TABLE public.{tempTableName} (LIKE public.{tableName} INCLUDING ALL);
                ALTER TABLE public.{tempTableName} ADD COLUMN Page INTEGER;
                ALTER TABLE public.{tempTableName} ADD COLUMN PageSize INTEGER;
                CREATE INDEX idx_{tempTableName}_{keyName} ON {tempTableName}({keyName});
            ");
            _logger.LogInformation("Создана временная таблица: {TempTableName}", tempTableName);

            // 2. Загружаем данные
            await LoadDataToTempTable(connection, tempTableName, keyName, batch, selectColumns);
            _logger.LogInformation("Данные загружены во временную таблицу: {TempTableName}", tempTableName);

            // 3. Синхронизируем с основной таблицей
            var savedCount = await connection.ExecuteAsync($@"
                INSERT INTO public.{tableName} ({selectColumns})
                SELECT {selectColumns} FROM public.{tempTableName}
                ON CONFLICT ({keyName}) DO UPDATE
                SET {string.Join(',', conflictColumns)};
            ");
            _logger.LogInformation("Данные из временной таблицы синхронизированны: {TempTableName}", tempTableName);


            // 4. Удаляем временную таблицу
            await connection.ExecuteAsync($"DROP TABLE public.{tempTableName};");
            _logger.LogInformation("Временная таблица удалена: {TempTableName}", tempTableName);

            return savedCount;
        }
        catch (Exception ex)
        {
            // Логируем ошибку, оставляем временную таблицу для анализа и продолжаем сохранение дальше
            _logger.LogError(ex, $"Ошибка в батче {batchId}: {ex.Message}");
            failBack?.Invoke(ex);
            return 0;
        }
    }

    private async Task LoadDataToTempTable<TEntity>(NpgsqlConnection connection, string tempTableName, string keyName,
        IEnumerable<TEntity> batch, string selectColumns) where TEntity : BaseEntity

    {
        var properties = typeof(TEntity)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .Where(p => string.Equals(keyName, "id", StringComparison.OrdinalIgnoreCase) ||
                        (!string.Equals(keyName, "id", StringComparison.OrdinalIgnoreCase) && 
                         !string.Equals(p.GetCustomAttribute<ColumnAttribute>()!.Name, "id", StringComparison.OrdinalIgnoreCase)))
            .ToDictionary(k => k.GetCustomAttribute<ColumnAttribute>()!.Name!, v => v);

        await using var writer = await connection.BeginBinaryImportAsync(
            $"COPY public.{tempTableName} ({selectColumns}) FROM STDIN (FORMAT BINARY)");

        foreach (TEntity entity in batch)
        {
            await writer.StartRowAsync();

            foreach (KeyValuePair<string, PropertyInfo> pair in properties)
            {
                await writer.WriteAsync(pair.Value.GetValue(entity), GetNpgsqlDbTypeFast(pair.Value.PropertyType), CancellationToken.None);
            }
        }

        await writer.CompleteAsync();
    }

    public static NpgsqlDbType GetNpgsqlDbTypeFast(Type propertyType)
    {
        if (TypeMap.TryGetValue(propertyType, out var npgsqlType))
        {
            return npgsqlType;
        }

        // Проверяем, является ли тип nullable и есть ли базовый тип в словаре
        var underlyingType = Nullable.GetUnderlyingType(propertyType);
        if (underlyingType != null && TypeMap.TryGetValue(underlyingType, out npgsqlType))
        {
            return npgsqlType;
        }

        // Проверяем массивы
        if (propertyType.IsArray)
        {
            var elementType = propertyType.GetElementType();
            if (elementType != null && TypeMap.TryGetValue(elementType, out var elementNpgsqlType))
            {
                return NpgsqlDbType.Array | elementNpgsqlType;
            }
        }

        return NpgsqlDbType.Text;
    }

    private static readonly Dictionary<Type, NpgsqlDbType> TypeMap = new()
    {
        // Целочисленные
        [typeof(int)] = NpgsqlDbType.Integer,
        [typeof(uint)] = NpgsqlDbType.Integer,
        [typeof(int?)] = NpgsqlDbType.Integer,  // Явно для nullable
        [typeof(uint?)] = NpgsqlDbType.Integer, // Явно для nullable

        [typeof(long)] = NpgsqlDbType.Bigint,
        [typeof(ulong)] = NpgsqlDbType.Bigint,
        [typeof(long?)] = NpgsqlDbType.Bigint,
        [typeof(ulong?)] = NpgsqlDbType.Bigint,

        [typeof(short)] = NpgsqlDbType.Smallint,
        [typeof(ushort)] = NpgsqlDbType.Smallint,
        [typeof(short?)] = NpgsqlDbType.Smallint,
        [typeof(ushort?)] = NpgsqlDbType.Smallint,

        // Числа с плавающей точкой
        [typeof(float)] = NpgsqlDbType.Real,
        [typeof(float?)] = NpgsqlDbType.Real,
        [typeof(double)] = NpgsqlDbType.Double,
        [typeof(double?)] = NpgsqlDbType.Double,
        [typeof(decimal)] = NpgsqlDbType.Numeric,
        [typeof(decimal?)] = NpgsqlDbType.Numeric,

        // Логический
        [typeof(bool)] = NpgsqlDbType.Boolean,
        [typeof(bool?)] = NpgsqlDbType.Boolean,

        // Строковые
        [typeof(string)] = NpgsqlDbType.Text,
        [typeof(char)] = NpgsqlDbType.Char,
        [typeof(char?)] = NpgsqlDbType.Char,

        // Дата и время
        [typeof(DateTime)] = NpgsqlDbType.Timestamp,
        [typeof(DateTime?)] = NpgsqlDbType.Timestamp,
        [typeof(DateTimeOffset)] = NpgsqlDbType.TimestampTz,
        [typeof(DateTimeOffset?)] = NpgsqlDbType.TimestampTz,
        [typeof(DateOnly)] = NpgsqlDbType.Date,
        [typeof(DateOnly?)] = NpgsqlDbType.Date,
        [typeof(TimeOnly)] = NpgsqlDbType.Time,
        [typeof(TimeOnly?)] = NpgsqlDbType.Time,
        [typeof(TimeSpan)] = NpgsqlDbType.Interval,
        [typeof(TimeSpan?)] = NpgsqlDbType.Interval,

        // Прочие
        [typeof(byte[])] = NpgsqlDbType.Bytea,
        [typeof(Guid)] = NpgsqlDbType.Uuid,
        [typeof(Guid?)] = NpgsqlDbType.Uuid,
    };
}