using InvestLens.Data.Entities;
using InvestLens.Data.Shared.Responses;
using InvestLens.Shared.Helpers;
using System.Text.Json;

namespace InvestLens.Data.Api.Converter;

public static class ResponseToEntityConverters
{
    public static List<Security> SecurityResponseToEntityConverter(SecuritiesResponse securitiesResponse)
    {
        var result = new List<Security>();

        foreach (object[] row in securitiesResponse.Securities.Data)
        {
            var security = new Security();
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
                propertyInfo.SetValue(security, value);
            }
            result.Add(security);
        }

        return result;
    }

    private static dynamic? GetValue(JsonElement? element, ColumnMetadata metadata, Type propertyType)
    {
        if (element is null) return null;
        switch (metadata.Type)
        {
            case "string":
                return element.Value.GetString();
            case "int32":
                var v = element.Value.GetInt32();
                return (propertyType == typeof(bool)) ? v > 0 : v;
            default:
                throw new Exception($"Unknown type: {metadata.Type}");
        }
    }
}