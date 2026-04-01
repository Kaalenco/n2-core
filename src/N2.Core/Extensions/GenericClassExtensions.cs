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
            static key =>
            {
                PropertyInfo[] sourceProperties = key.Item1.GetProperties();
                PropertyInfo[] targetProperties = key.Item2.GetProperties();
                return sourceProperties
                    .Where(sp => sp.CanRead)
                    .Select(sp =>
                    {
                        PropertyInfo? tp = Array.Find(targetProperties, x => string.Equals(x.Name, sp.Name, StringComparison.OrdinalIgnoreCase));
                        if (tp == null || !tp.CanWrite)
                            return ((PropertyInfo Source, PropertyInfo? Target))(sp, null);
#pragma warning disable RCS1146 // Use conditional access
                        MethodInfo? setMethod = tp.GetSetMethod();
                        if (setMethod == null || setMethod.IsPrivate || setMethod.IsFamily)
                            return ((PropertyInfo Source, PropertyInfo? Target))(sp, null);
#pragma warning restore RCS1146 // Use conditional access
                        return ((PropertyInfo Source, PropertyInfo? Target))(sp, tp);
                    })
                    .ToArray();
            });

        foreach ((PropertyInfo sp, PropertyInfo? tp) in mappings)
        {
            if (tp != null)
                tp.SetValue(t, sp.GetValue(s));
        }
    }

    public static string SerializeForView<T>(this T t)
    {
        return JsonSerializer.Serialize(t, options);
    }
}