namespace N2.Core.Validators;

public class NotNullValidator<T> : IRuntimeValidator<T>
{
    /// <summary>
    /// Validates the.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Validate(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
    }
}