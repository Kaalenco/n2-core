using System.Text.Json;

namespace N2.Core.Extensions;

public static class GenericClassExtensions
{
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = true
    };

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
        var sourceType = typeof(TSource);
        var targetType = typeof(TTarget);
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(t);
        var sourceProperties = sourceType.GetProperties();
        var targetProperties = targetType.GetProperties();
        foreach (var sp in sourceProperties)
        {
            if (!sp.CanRead)
            {
                continue;
            }

            var tp = Array.Find(targetProperties, x => string.Equals(x.Name, sp.Name, StringComparison.OrdinalIgnoreCase));
            if (tp != null)
            {
                if (!tp.CanWrite)
                {
                    continue;
                }
                var setMethod = tp.GetSetMethod();
#pragma warning disable RCS1146 // Use conditional access
                if (setMethod == null || setMethod.IsPrivate || setMethod.IsFamily)
                {
                    continue;
                }
#pragma warning restore RCS1146 // Use conditional access
                var value = sp.GetValue(s);
                tp.SetValue(t, value);
            }
        }
    }

    public static string SerializeForView<T>(this T t)
    {
        return JsonSerializer.Serialize(t, options);
    }
}