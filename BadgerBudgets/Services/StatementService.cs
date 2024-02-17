using BadgerBudgets.Models;
using Blazorise.Extensions;

namespace BadgerBudgets.Services;

public class StatementService
{
    public static List<StatementItem> Items { get; set; } = new();
    private static Dictionary<ColumnType, int> ColumnMappings = new();
    public static bool HasContent => Items.Count != 0;
    public static bool HasMappings => ColumnMappings.Count > 0;

    public void SetMappings(Dictionary<ColumnType, int> mappings)
    {
        ColumnMappings = mappings;
    }

    public void RemoveMapping(ColumnType type)
    {
        if (ColumnMappings.ContainsKey(type))
            ColumnMappings.Remove(type);
    }

    public void RemoveMapping(int index)
    {
        if (!ColumnMappings.ContainsValue(index)) return;
        
        var valueIndex = ColumnMappings.Values.Index(x => x == index);
        var keyIndex = ColumnMappings.Keys.ToArray()[valueIndex];
        ColumnMappings.Remove(keyIndex);
    }
    
    public void UpdateMapping(ColumnType type, int columnIndex)
    {
        ColumnMappings[type] = columnIndex;
    }

    public string SerializeMappings()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(ColumnMappings);
    }

    public Dictionary<string, List<double>> GetAmountsByCategory(bool ignoreCredit=true)
    {
        return Items.GroupBy(x => x.Category)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Amount).ToList());
    }
    
    public void ParseFile(string[] lines, char delimiter = ',')
    {
        foreach (var line in lines)
        {
            var parts = line.Split(delimiter);
            if (parts.Length < 5)
                continue;
            
            DateOnly.TryParse(parts[ColumnMappings[ColumnType.TransactionDate]], out var transactionDate);
            double.TryParse(parts[ColumnMappings[ColumnType.Amount]], out var amount);
            var description = parts[ColumnMappings[ColumnType.LineItem]];
            var category = parts[ColumnMappings[ColumnType.Category]];

            var isDebit = true;
            if (ColumnMappings.ContainsKey(ColumnType.Debit))
            {
                isDebit = parts[ColumnMappings[ColumnType.Debit]]
                    .Contains("debit", StringComparison.InvariantCultureIgnoreCase);
            }
            else if (ColumnMappings.ContainsKey(ColumnType.CreditDebitCombined))
            {
                isDebit = parts[ColumnMappings[ColumnType.CreditDebitCombined]]
                    .Contains("debit", StringComparison.InvariantCultureIgnoreCase);
            }

            if (transactionDate == DateOnly.MinValue ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(category) ||
                amount == 0)
                continue;
            
            Items.Add(new()
            {
                Amount = amount,
                LineItem = description,
                Category = category,
                Date = transactionDate,
                IsDebit = isDebit
            });
        }
    }
}