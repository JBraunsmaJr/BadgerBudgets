namespace BadgerBudgets.Models;

public class StatementItem
{
    public DateOnly Date { get; set; }
    public string LineItem { get; set; }
    public string OriginalLineItem { get; set; }
    public double Amount { get; set; }
    public bool IsDebit { get; set; }
    public bool IsCredit => !IsDebit;
    public string Category { get; set; }
    public string OriginalCategory { get; set; }

    public override string ToString() 
        => $"Date: {Date.ToString()} | Desc: {LineItem} | Original: {OriginalLineItem} | Amount: {Amount} | Category: {OriginalCategory}";


    public static HashSet<string> CreditCategories = new()
    {
        "Deposits",
        "Interest"
    };

    public string GetColumnValue(ColumnType type) => type switch
    {
        ColumnType.TransactionDate => Date.ToString(),
        ColumnType.Amount => Amount.ToString("C"),
        ColumnType.LineItem => OriginalLineItem,
        _ => OriginalCategory
    };

    public void UpdateValue(ColumnType type, string value)
    {
        switch (type)
        {
            default:
                Console.WriteLine($"Updating Category from {OriginalCategory} to {value}");
                Category = value;
                break;
            case ColumnType.TransactionDate:
                Console.WriteLine($"Updating Date from {Date.ToString()} to {value}");
                Date = DateOnly.Parse(value);
                break;
            case ColumnType.LineItem:
                Console.WriteLine($"Updating Desc from {OriginalLineItem} to {value}");
                LineItem = value;
                break;
            case ColumnType.Amount:
                if (double.TryParse(value, out var amount))
                    Amount = amount;
                break;
                    
        }
    }
}