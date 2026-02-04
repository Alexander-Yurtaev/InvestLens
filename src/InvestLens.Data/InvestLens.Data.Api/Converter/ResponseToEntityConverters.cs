using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Index;
using InvestLens.Data.Shared.Responses;
using InvestLens.Shared.Helpers;
using System.Text.Json;

namespace InvestLens.Data.Api.Converter;

public static class ResponseToEntityConverters
{
    public static List<BaseEntity> ResponseToEntityConverter(IBaseResponse response, int page, int pageSize)
    {
        return response switch
        {
            SecuritiesResponse securitiesResponse => SecurityResponseToEntityConverter(securitiesResponse, page, pageSize),
            EngineIndexDataResponse engineIndexDataResponse => SectionIndexDataResponseToEntityConverter<Engine>(engineIndexDataResponse.Section),
            MarketIndexDataResponse marketIndexDataResponse => SectionIndexDataResponseToEntityConverter<Market>(marketIndexDataResponse.Section),
            BoardIndexDataResponse boardIndexDataResponse => SectionIndexDataResponseToEntityConverter<Board>(boardIndexDataResponse.Section),
            BoardGroupIndexDataResponse boardGroupIndexDataResponse => SectionIndexDataResponseToEntityConverter<BoardGroup>(boardGroupIndexDataResponse.Section),
            DurationIndexDataResponse durationIndexDataResponse => SectionIndexDataResponseToEntityConverter<Duration>(durationIndexDataResponse.Section),
            SecurityTypeIndexDataResponse securityTypeIndexDataResponse => SectionIndexDataResponseToEntityConverter<SecurityType>(securityTypeIndexDataResponse.Section),
            SecurityGroupIndexDataResponse securityGroupIndexDataResponse => SectionIndexDataResponseToEntityConverter<SecurityGroup>(securityGroupIndexDataResponse.Section),
            SecurityCollectionIndexDataResponse securityCollectionIndexDataResponse => SectionIndexDataResponseToEntityConverter<SecurityCollection>(securityCollectionIndexDataResponse.Section),
            _ => throw new ArgumentException($"Unknown response type: {response.GetType()}")
        };
    }

    #region Private Methods

    private static List<BaseEntity> SecurityResponseToEntityConverter(SecuritiesResponse securitiesResponse, int page, int pageSize)
    {
        var result = new List<Security>();

        foreach (object[] row in securitiesResponse.Section.Data)
        {
            var security = new SecurityEx
            {
                Page = page,
                PageSize = pageSize
            };

            for (int i = 0; i < securitiesResponse.Section.Columns.Length; i++)
            {
                var column = securitiesResponse.Section.Columns[i];
                var metaData = securitiesResponse.Section.Metadata[column];
                var propertyInfo = PropertyHelper.GetPropertyByColumnCached<Security>(column);

                if (propertyInfo is null)
                {
                    throw new InvalidDataException($"Invalid property '{column}'");
                }

                var value = GetValue((JsonElement?)row[i], metaData, propertyInfo.PropertyType);
                propertyInfo.SetValue(security, value);
            }
            result.Add(security);
        }

        return result.Cast<BaseEntity>().ToList();
    }

    private static List<BaseEntity> SectionIndexDataResponseToEntityConverter<TEntity>(Section section) where TEntity : IndexBaseEntity
    {
        var result = new List<TEntity>();

        foreach (object[] row in section.Data)
        {
            var entity = Activator.CreateInstance<TEntity>();

            for (int i = 0; i < section.Columns.Length; i++)
            {
                var column = section.Columns[i];
                var metaData = section.Metadata[column];
                var propertyInfo = PropertyHelper.GetPropertyByColumnCached<TEntity>(column);

                if (propertyInfo is null)
                {
                    throw new InvalidDataException($"Invalid property '{column}'");
                }

                var value = GetValue((JsonElement?)row[i], metaData, propertyInfo.PropertyType);
                propertyInfo.SetValue(entity, value);
            }
            result.Add(entity);
        }

        return result.Cast<BaseEntity>().ToList();
    }

    private static dynamic? GetValue(JsonElement? element, ColumnMetadata metadata, Type propertyType)
    {
        if (string.Equals(metadata.Type, "string", StringComparison.OrdinalIgnoreCase))
        {
            if (element is null) return "";
            return element.Value.GetString() ?? "";
        }

        if (string.Equals(metadata.Type, "int32", StringComparison.OrdinalIgnoreCase))
        {
            if (element is null) return null;

            var v = element.Value.GetInt32();
            return (propertyType == typeof(bool)) ? v > 0 : v;
        }

        throw new Exception($"Unknown type: {metadata.Type}");
    }

    #endregion Private Methods
}