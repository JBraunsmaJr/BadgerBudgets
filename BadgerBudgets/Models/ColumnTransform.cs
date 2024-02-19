namespace BadgerBudgets.Models;

public enum TransformCondition
{
    Contains,
    Equals
}

public class ConditionalColumnTransform
{
    public ColumnType ColumnType { get; set; }
    public string? Value { get; set; }
    public TransformCondition Condition { get; set; }
    
    public bool IsValid(string? incomingValue)
    {
        if (string.IsNullOrWhiteSpace(Value) && string.IsNullOrWhiteSpace(incomingValue))
            return true;

        if (string.IsNullOrWhiteSpace(incomingValue))
            return false;

        return Condition switch
        {
            TransformCondition.Contains => incomingValue.Contains(Value, StringComparison.InvariantCultureIgnoreCase),
            _ => incomingValue.Equals(Value, StringComparison.InvariantCultureIgnoreCase)
        };
    }
}

public class ColumnTransform
{
    /// <summary>
    /// Category from <see cref="SourceMaterial"/> to transform
    /// </summary>
    public string SourceValue { get; set; }
    
    /// <summary>
    /// Category to transform <see cref="SourceValue"/> into
    /// </summary>
    public string DestinationValue { get; set; }

    public ColumnType Type { get; set; }
    public TransformCondition Condition { get; set; }

    /// <summary>
    /// Optional condition where another columns value influences outcome of transform
    /// </summary>
    public ConditionalColumnTransform? ColumnCondition { get; set; }
    
    public bool IsValid(string? incomingValue)
    {
        if (string.IsNullOrWhiteSpace(SourceValue) && string.IsNullOrWhiteSpace(incomingValue))
            return true;

        if (string.IsNullOrWhiteSpace(incomingValue))
            return false;

        return Condition switch
        {
            TransformCondition.Contains => incomingValue.Contains(SourceValue, StringComparison.InvariantCultureIgnoreCase),
            _ => incomingValue.Equals(SourceValue, StringComparison.InvariantCultureIgnoreCase)
        };
    }
}