using InvestLens.Data.Shared.Responses;
using InvestLens.Shared.Helpers;
using System.Text.Json;
using InvestLens.Data.Entities;

namespace InvestLens.Data.Api.Converter;

public static class ResponseToEntityConverters
{
    public static List<BaseEntity> ResponseToEntityConverter(IBaseResponse response, int page, int pageSize)
    {
        return response switch
        {
            SecuritiesResponse securitiesResponse => SecurityResponseToEntityConverter(securitiesResponse, page, pageSize),
            IndexDataResponse indexDataResponse => IndexDataResponseToEntityConverter(indexDataResponse, page, pageSize),
            _ => throw new ArgumentException($"Unknown response type: {response.GetType()}")
        };
    }

    #region Private Methods

    private static List<BaseEntity> SecurityResponseToEntityConverter(SecuritiesResponse securitiesResponse, int page, int pageSize)
    {
        var result = new List<Security>();

        foreach (object[] row in securitiesResponse.Securities.Data)
        {
            var security = new SecurityEx
            {
                Page = page,
                PageSize = pageSize
            };

            for (int i = 0; i < securitiesResponse.Securities.Columns.Length; i++)
            {
                var column = securitiesResponse.Securities.Columns[i];
                var metaData = securitiesResponse.Securities.Metadata[column];
                var propertyInfo = PropertyHelper.GetPropertyByJsonNameCached<Security>(column);

                if (propertyInfo is null)
                {
                    throw new InvalidDataException($"Invalid property '{column}'");
                }

                var value = GetValue((JsonElement?)row[i], metaData, propertyInfo.PropertyType);
                if (value is null && string.Compare(column, "shortname", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    System.Diagnostics.Debugger.Break();
                }
                propertyInfo.SetValue(security, value);
            }
            result.Add(security);
        }

        return result.Cast<BaseEntity>().ToList();
    }

    private static List<BaseEntity> IndexDataResponseToEntityConverter(IndexDataResponse indexDataResponse, int page, int pageSize)
    {
        throw new NotImplementedException();
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