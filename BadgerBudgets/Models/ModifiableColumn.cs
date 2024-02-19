using System.Collections;

namespace BadgerBudgets.Models;

/// <summary>
/// A column which can be modified, but maintain reference to original value
/// </summary>
/// <typeparam name="T">Type of data stored within column</typeparam>
public class ModifiableColumn<T> where T : 
    IComparable,
    IEnumerable,
    IConvertible,
    IComparable<T?>,
    IEquatable<T?>
{
    public T OriginalValue { get; init; }
    public T Value { get; set; }
    public bool HasChanged => !Value.Equals(OriginalValue);
    public override string ToString()
        => $"[Current: {Value} | Original: {OriginalValue}]";
}