namespace N2.Core.Extensions;

public static class ArrayExtensions
{
    public static void Clear<T>(this T[] array) where T : struct
    {
        if (array == null)
        {
            return;
        }

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = default;
        }
    }
}
