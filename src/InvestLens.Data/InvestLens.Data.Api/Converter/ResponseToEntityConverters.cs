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
            IndexDataResponse indexDataResponse => IndexDataResponseToEntityConverter(indexDataResponse),
            _ => throw new ArgumentException($"Unknown response type: {response.GetType()}")
        };
    }

    #region Private Methods

    private static List<BaseEntity> SecurityResponseToEntityConverter(SecuritiesResponse securitiesResponse, int page, int pageSize)
    {
        var result = new List<Security>();

        foreach (object[] row in securitiesResponse.Sections["securities"].Data)
        {
            var security = new SecurityEx
            {
                Page = page,
                PageSize = pageSize
            };

            for (int i = 0; i < securitiesResponse.Sections["securities"].Columns.Length; i++)
            {
                var column = securitiesResponse.Sections["securities"].Columns[i];
                var metaData = securitiesResponse.Sections["securities"].Metadata[column];
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

    private static List<BaseEntity> IndexDataResponseToEntityConverter(IndexDataResponse indexDataResponse)
    {
        var result = new List<Engine>();

        foreach (object[] row in indexDataResponse.Engines.Data)
        {
            var engine = new Engine();

            for (int i = 0; i < indexDataResponse.Engines.Columns.Length; i++)
            {
                var column = indexDataResponse.Engines.Columns[i];
                var metaData = indexDataResponse.Engines.Metadata[column];
                var propertyInfo = PropertyHelper.GetPropertyByColumnCached<Engine>(column);

                if (propertyInfo is null)
                {
                    throw new InvalidDataException($"Invalid property '{column}'");
                }

                var value = GetValue((JsonElement?)row[i], metaData, propertyInfo.PropertyType);
                propertyInfo.SetValue(engine, value);
            }
            result.Add(engine);
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