using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace N2.Core.Extensions;

public static class GenericClassExtensions
{
    private static readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true
    };

    private static readonly ConcurrentDictionary<(Type, Type), (PropertyInfo Source, PropertyInfo? Target)[]> _mappingCache = new();

    public static TTarget? CopyFrom<TSource, TTarget>(this TTarget t, TSource s)
        where TTarget : class
        where TSource : class
    {
        if (t == null)
        {
            return default;
        }

        s.MapPropertyValuesByName(t);
        return t;
    }

    public static void MapPropertyValuesByName<TSource, TTarget>(this TSource s, TTarget t)
    {
        Contract.NotNull(s, nameof(s));
        Contract.NotNull(t, nameof(t));

        (PropertyInfo Source, PropertyInfo? Target)[] mappings = _mappingCache.GetOrAdd(
            (typeof(TSource), typeof(TTarget)),
            static key => BuildMappings(key.Item1, key.Item2));

        foreach ((PropertyInfo sp, PropertyInfo? tp) in mappings)
        {
            if (tp != null)
                tp.SetValue(t, sp.GetValue(s));
        }
    }

    /// <summary>
    /// Pairs every readable source property with the writable target property of the same
    /// name (case-insensitive), or with null when there is none.
    /// </summary>
    private static (PropertyInfo Source, PropertyInfo? Target)[] BuildMappings(Type sourceType, Type targetType)
    {
        PropertyInfo[] targetProperties = targetType.GetProperties();
        return sourceType.GetProperties()
            .Where(sp => sp.CanRead)
            .Select(sp => (Source: sp, Target: FindWritableTarget(targetProperties, sp.Name)))
            .ToArray();
    }

    /// <summary>
    /// Finds the target property with the given name (case-insensitive) that has a public
    /// setter; returns null when there is none.
    /// </summary>
    private static PropertyInfo? FindWritableTarget(PropertyInfo[] targetProperties, string name)
    {
        PropertyInfo? tp = Array.Find(targetProperties, x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (tp == null || !tp.CanWrite)
        {
            return null;
        }
#pragma warning disable RCS1146 // Use conditional access
        MethodInfo? setMethod = tp.GetSetMethod();
        if (setMethod == null || setMethod.IsPrivate || setMethod.IsFamily)
        {
            return null;
        }
#pragma warning restore RCS1146 // Use conditional access
        return tp;
    }

    public static string SerializeForView<T>(this T t)
    {
        return JsonSerializer.Serialize(t, options);
    }
}