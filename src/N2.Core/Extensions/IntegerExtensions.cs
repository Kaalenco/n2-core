
namespace N2.Core.Extensions;
public static class IntegerExtensions
{
    public static bool IsEven(this int value) => value % 2 == 0;
    public static bool IsOdd(this int value) => value % 2 != 0;
    public static bool IsPositive(this int value) => value > 0;
    public static bool IsNegative(this int value) => value < 0;
    public static bool IsZero(this int value) => value == 0;
    public static bool IsBetween(this int value, int min, int max) => value >= min && value <= max;
    public static bool IsSuccessCode(this int value) => value >= 200 && value < 300;
    public static bool IsErrorCode(this int value) => value >= 400 && value < 600;
}
