namespace BadgerBudgets.Models;

/// <summary>
/// A column which can be modified, but maintain reference to original value
/// </summary>
/// <typeparam name="T">Type of data stored within column</typeparam>
public class ModifiableColumn<T>
{
    public T OriginalValue { get; init; }
    public T Value { get; set; }

    public override string ToString()
        => $"[Current: {Value} | Original: {OriginalValue}]";
}