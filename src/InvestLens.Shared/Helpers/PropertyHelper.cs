using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;

namespace InvestLens.Shared.Helpers;

public static class PropertyHelper
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _cache = new();

    public static PropertyInfo? GetPropertyByJsonName<T>(string jsonPropertyName)
    {
        return GetPropertyByJsonName(typeof(T), jsonPropertyName);
    }

    public static PropertyInfo? GetPropertyByJsonName(Type type, string jsonPropertyName)
    {
        return type.GetProperties()
            .FirstOrDefault(prop =>
            {
                var attribute = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                return attribute?.Name == jsonPropertyName;
            });
    }

    public static PropertyInfo? GetPropertyByJsonNameCached<T>(string jsonPropertyName)
    {
        var type = typeof(T);

        if (!_cache.TryGetValue(type, out var propertyMap))
        {
            propertyMap = type.GetProperties()
                .Where(prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
                .ToDictionary(
                    prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name,
                    prop => prop
                );

            _cache.TryAdd(type, propertyMap);
        }

        return propertyMap.GetValueOrDefault(jsonPropertyName);
    }
}