using BadgerBudgets.Models;
using Blazored.LocalStorage;
using Newtonsoft.Json;

namespace BadgerBudgets.Services;

public class StatementService
{
    public static List<StatementItem> Items { get; set; } = new();
    public static Dictionary<string, SourceMaterial> SourceMaterials { get; set; } = new();
    
    private readonly ILogger<StatementService> _logger;

    private const string MaterialsKey = "Materials";
    private readonly ILocalStorageService _storage;

    public StatementService(ILocalStorageService storage, ILogger<StatementService> logger)
    {
        _logger = logger;
        _storage = storage;
        Task.Run(LoadFromStorage);
    }

    public async Task Save()
    {
        await _storage.SetItemAsStringAsync(MaterialsKey,SerializeMappings());
    }
    
    public async Task LoadFromStorage()
    {
        if (!await _storage.ContainKeyAsync(MaterialsKey))
            return;
        
        try
        {
            var json = await _storage.GetItemAsStringAsync(MaterialsKey);

            if (!string.IsNullOrWhiteSpace(json))
            {
                var materials = JsonConvert.DeserializeObject<SourceMaterial[]>(json);
                
                if(materials is not null)
                    SourceMaterials = materials.ToDictionary(x => x.Name, x => x);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load from storage: {Message}", ex);
        }
    }

    public byte[] SaveToFile() 
        => System.Text.Encoding.UTF8.GetBytes(SerializeMappings());

    private string SerializeMappings() 
        => JsonConvert.SerializeObject(SourceMaterials.Values.ToArray());

    public Dictionary<string, List<double>> GetAmountsByCategory(bool ignoreCredit = true)
    {
        return Items.GroupBy(x => x.Category)
            .Where(x => !StatementItem.CreditCategories.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Select(y => y.Amount).ToList());
    }

    /// <summary>
    /// Reapplies transforms to existing data
    /// </summary>
    public void ApplyTransforms()
    {
        foreach (var source in SourceMaterials)
        {
            Console.WriteLine($"Updating values in {source.Key}");
            source.Value.ApplyTransforms();
        }
    }
    
    public void ParseFile(string sourceName, string[] lines, char delimiter = ',')
    {
        if (!SourceMaterials.ContainsKey(sourceName))
            return;

        var source = SourceMaterials[sourceName];
        var mappings = source.Mappings;
        
        foreach (var line in lines)
        {
            var parts = line.Split(delimiter);
            if (parts.Length < 5)
                continue;
            
            var originalDescription = parts[mappings[ColumnType.LineItem]];
            var originalCategory = parts[mappings[ColumnType.Category]];
            
            parts = source.ApplyTransforms(parts);
            
            DateOnly.TryParse(parts[mappings[ColumnType.TransactionDate]], out var transactionDate);
            double.TryParse(parts[mappings[ColumnType.Amount]], out var amount);
            var description = parts[mappings[ColumnType.LineItem]];
            var category = parts[mappings[ColumnType.Category]];
            
            var isDebit = true;
            if (mappings.TryGetValue(ColumnType.Debit, out var mapping))
            {
                isDebit = parts[mapping]
                    .Contains("debit", StringComparison.InvariantCultureIgnoreCase);
            }
            else if (mappings.TryGetValue(ColumnType.CreditDebitCombined, out var columnMapping))
            {
                isDebit = parts[columnMapping]
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
                IsDebit = isDebit,
                OriginalLineItem = originalDescription,
                OriginalCategory = originalCategory
            });
        }
    }
}