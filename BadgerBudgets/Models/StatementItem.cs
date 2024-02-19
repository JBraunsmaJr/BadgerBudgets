namespace BadgerBudgets.Models;

public class StatementItem : IEquatable<StatementItem>
{
    public DateOnly Date { get; set; }
    public ModifiableColumn<string> Description { get; set; }
    public double Amount { get; set; }
    public bool IsDebit { get; set; }
    public bool IsCredit => !IsDebit;

    public ModifiableColumn<string> Category { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StatementItem)obj);
    }

    public bool Equals(StatementItem? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Date.Equals(other.Date) && 
               Description.Value.Equals(other.Description.Value) && Amount.Equals(other.Amount) && 
               IsDebit == other.IsDebit && 
               Category.Value.Equals(other.Category.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Date, Description.Value, Amount, IsDebit, Category.Value);
    }

    public override string ToString() 
        => $"Date: {Date.ToString()} | Desc: {Description} | Amount: {Amount} | Category: {Category}";


    public static HashSet<string> CreditCategories = new()
    {
        "Deposits",
        "Interest"
    };

    public string GetColumnValue(ColumnType type) => type switch
    {
        ColumnType.TransactionDate => Date.ToString(),
        ColumnType.Amount => Amount.ToString("C"),
        ColumnType.LineItem => Description.OriginalValue,
        _ => Category.OriginalValue
    };

    public void UpdateValue(ColumnType type, string value)
    {
        switch (type)
        {
            default:
                Console.WriteLine($"Updating Category from {Category} to {value}");
                Category.Value = value;
                break;
            case ColumnType.TransactionDate:
                Console.WriteLine($"Updating Date from {Date.ToString()} to {value}");
                Date = DateOnly.Parse(value);
                break;
            case ColumnType.LineItem:
                Console.WriteLine($"Updating Desc from {Description} to {value}");
                Description.Value = value;
                break;
            case ColumnType.Amount:
                if (double.TryParse(value, out var amount))
                    Amount = amount;
                break;
                    
        }
    }
}