using System.Text.Json;

namespace N2.Core.Extensions;

public static class GenericClassExtensions
{
    private static readonly JsonSerializerOptions options = new()
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
        Type sourceType = typeof(TSource);
        Type targetType = typeof(TTarget);
        Contract.NotNull(s, nameof(s));
        Contract.NotNull(t, nameof(t));
        System.Reflection.PropertyInfo[] sourceProperties = sourceType.GetProperties();
        System.Reflection.PropertyInfo[] targetProperties = targetType.GetProperties();
        foreach (System.Reflection.PropertyInfo? sp in sourceProperties)
        {
            if (!sp.CanRead)
            {
                continue;
            }

            System.Reflection.PropertyInfo? tp = Array.Find(targetProperties, x => string.Equals(x.Name, sp.Name, StringComparison.OrdinalIgnoreCase));
            if (tp != null)
            {
                if (!tp.CanWrite)
                {
                    continue;
                }
                System.Reflection.MethodInfo? setMethod = tp.GetSetMethod();
#pragma warning disable RCS1146 // Use conditional access
                if (setMethod == null || setMethod.IsPrivate || setMethod.IsFamily)
                {
                    continue;
                }
#pragma warning restore RCS1146 // Use conditional access
                object? value = sp.GetValue(s);
                tp.SetValue(t, value);
            }
        }
    }

    public static string SerializeForView<T>(this T t)
    {
        return JsonSerializer.Serialize(t, options);
    }
}