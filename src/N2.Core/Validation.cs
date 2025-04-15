using System.Collections.ObjectModel;

namespace N2.Core;

public class Validator : IValidation
{
    private readonly List<ValidationResult> results = [];
    public ReadOnlyCollection<ValidationResult> Results => results.AsReadOnly();
    public bool Valid => results.Count == 0;
    public void LengthMustBeEqualTo<T>(string value, int length, string message) where T : class
    {
        NotNullOrEmpty<T>(value, message);
        if (value.Length != length)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.StringLenght });
        }
    }

    public void MinimumValue<T>(int value, int minimum, string message) where T : class
    {
        if (value < minimum)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.MinimumValue });
        }
    }

    public void NotNullOrEmpty<T>(string value, string message) where T : class
    {
        if (string.IsNullOrEmpty(value))
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.ValueNullOrEmpty });
        }
    }

    public void StartLowerThenEnd<T>(int start, int end, string message) where T : class
    {
        if (start >= end)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.StartLowerThenEnd });
        }
    }

    public void StartLowerThenEnd<T>(DateTime start, DateTime end, string message) where T : class
    {
        if (start >= end)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.StartLowerThenEnd });
        }
    }

    public void ZeroOrPositive<T>(int value, string message) where T : class
    {
        if (value < 0)
        {
            results.Add(new ValidationResult { Message = message, TypeName = typeof(T).Name, ErrorCode = ErrorCode.MinimumValue });
        }
    }
}