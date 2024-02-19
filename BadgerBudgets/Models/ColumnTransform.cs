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

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((ColumnTransform)obj);
    }

    private bool Equals(ColumnTransform other) 
        => SourceValue == other.SourceValue && 
           DestinationValue == other.DestinationValue && 
           Type == other.Type && 
           Condition == other.Condition && 
           Equals(ColumnCondition, other.ColumnCondition);

    public override int GetHashCode()
    {
        return HashCode.Combine(SourceValue, DestinationValue, (int)Type, (int)Condition, ColumnCondition);
    }

    public override string ToString()
    {
        var text = $"if {Type.GetName()} {(Condition == TransformCondition.Contains ? "has" : "equals")} {SourceValue}";

        if (ColumnCondition is not null)
            text += $" and if {ColumnCondition.ColumnType.GetName()} {(ColumnCondition.Condition == TransformCondition.Contains ? "has" : "equals")} {ColumnCondition.Value}";

        return text + $" --> {DestinationValue}";
    }

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