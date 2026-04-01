using System.Collections.ObjectModel;

namespace N2.Core;

/// <summary>
/// Accumulates validation rule violations for a single operation.
/// After running all checks, inspect <see cref="Valid"/> or <see cref="Results"/>.
/// Each failed check appends a <see cref="ValidationResult"/> tagged with the type name
/// supplied via the type parameter.
/// </summary>
public class Validator : IValidation
{
    private readonly List<ValidationResult> results = [];

    /// <summary>All violations collected so far.</summary>
    public ReadOnlyCollection<ValidationResult> Results => results.AsReadOnly();

    /// <summary><c>true</c> when no violations have been recorded.</summary>
    public bool Valid => results.Count == 0;

    /// <summary>
    /// Checks that <paramref name="value"/> is not null or empty, then that its length
    /// equals <paramref name="length"/>. Both checks use the same <paramref name="message"/>.
    /// </summary>
    public void LengthMustBeEqualTo<T>(string value, int length, string message) where T : class
    {
        NotNullOrEmpty<T>(value, message);
        if (value!=null && value.Length != length)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.StringLenght });
        }
    }

    /// <summary>
    /// Checks that <paramref name="value"/> is greater than or equal to <paramref name="minimum"/>.
    /// </summary>
    public void MinimumValue<T>(int value, int minimum, string message) where T : class
    {
        if (value < minimum)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.MinimumValue });
        }
    }

    /// <summary>
    /// Checks that <paramref name="value"/> is not null or empty.
    /// </summary>
    public void NotNullOrEmpty<T>(string value, string message) where T : class
    {
        if (string.IsNullOrEmpty(value))
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.ValueNullOrEmpty });
        }
    }

    /// <summary>
    /// Checks that <paramref name="start"/> is strictly less than <paramref name="end"/>.
    /// </summary>
    public void StartLowerThenEnd<T>(int start, int end, string message) where T : class
    {
        if (start >= end)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.StartLowerThenEnd });
        }
    }

    /// <summary>
    /// Checks that <paramref name="start"/> is strictly earlier than <paramref name="end"/>.
    /// </summary>
    public void StartLowerThenEnd<T>(DateTime start, DateTime end, string message) where T : class
    {
        if (start >= end)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.StartLowerThenEnd });
        }
    }

    /// <summary>
    /// Checks that <paramref name="value"/> is zero or positive (≥ 0).
    /// </summary>
    public void ZeroOrPositive<T>(int value, string message) where T : class
    {
        if (value < 0)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.MinimumValue });
        }
    }
}