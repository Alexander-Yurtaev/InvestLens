using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace InvestLens.Shared.Helpers;

public static class PropertyHelper
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> Cache = new();

    public static PropertyInfo? GetPropertyByColumnCached<T>(string jsonPropertyName)
    {
        var type = typeof(T);

        if (!Cache.TryGetValue(type, out var propertyMap))
        {
            propertyMap = type.GetProperties()
                .Where(prop => !string.IsNullOrEmpty(prop.GetCustomAttribute<ColumnAttribute>()?.Name))
                .ToDictionary(
                    prop => prop.GetCustomAttribute<ColumnAttribute>()!.Name!,
                    prop => prop
                );

            Cache.TryAdd(type, propertyMap);
        }

        return propertyMap.GetValueOrDefault(jsonPropertyName);
    }
}