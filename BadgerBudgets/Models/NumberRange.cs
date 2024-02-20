using System.Numerics;

namespace BadgerBudgets.Models;

public struct NumberRange<T>
    where T : INumber<T>
{
    public T? Min { get; set; }
    public T? Max { get; set; }
}