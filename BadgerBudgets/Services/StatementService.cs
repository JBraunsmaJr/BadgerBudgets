using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BadgerBudgets.Models;
using Blazored.LocalStorage;
using CsvHelper;
using CsvHelper.Configuration;
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
        => Encoding.UTF8.GetBytes(SerializeMappings());

    private string SerializeMappings() 
        => JsonConvert.SerializeObject(SourceMaterials.Values.ToArray());

    public Dictionary<string, List<double>> GetAmountsByCategory(bool ignoreCredit = true)
    {
        return Items.GroupBy(x => x.Category)
            .Where(x => !StatementItem.CreditCategories.Contains(x.Key.Value))
            .ToDictionary(x => x.Key.Value, x => x.Select(y => y.Amount).ToList());
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
    
    public async Task ParseFile(string sourceName, TextReader textReader)
    {
        if (!SourceMaterials.ContainsKey(sourceName))
        {
            Console.WriteLine($"Source materials do not have source for {sourceName}");
            return;
        }

        Console.WriteLine(sourceName);
        var source = SourceMaterials[sourceName];
        var mappings = source.Mappings;

        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            Delimiter = source.Delimiter switch
            {
                DelimiterType.Comma => ",",
                DelimiterType.Tab => "\t"
            },
            HasHeaderRecord = true
        };
            
        using var csvHelper = new CsvReader(textReader, csvConfig);
        await csvHelper.ReadAsync();            
        csvHelper.ReadHeader();
        
        while(await csvHelper.ReadAsync())
        {
            var parts = new string[csvHelper.HeaderRecord.Length];

            var originalDescription = csvHelper.GetField<string>(mappings[ColumnType.LineItem]);
            var originalCategory = csvHelper.GetField<string>(mappings[ColumnType.Category]);
            
            parts = source.ApplyTransforms(parts);
            
            DateOnly.TryParse(csvHelper.GetField<string>(mappings[ColumnType.TransactionDate]), out var transactionDate);
            var number = Regex.Replace(csvHelper.GetField<string>(mappings[ColumnType.Amount]), @"[$,]", string.Empty);
            double.TryParse(number, NumberStyles.Currency, CultureInfo.CurrentCulture, out var amount);
            var description = csvHelper.GetField<string>(mappings[ColumnType.LineItem]);
            var category = csvHelper.GetField<string>(mappings[ColumnType.Category]);
            
            var isDebit = true;
            if (mappings.TryGetValue(ColumnType.Debit, out var mapping))
                isDebit = csvHelper.GetField<string>(parts[mapping]).Contains("debit", StringComparison.InvariantCultureIgnoreCase);
            else if (mappings.TryGetValue(ColumnType.CreditDebitCombined, out var columnMapping))
                isDebit = csvHelper.GetField<string>(columnMapping).Contains("debit", StringComparison.InvariantCultureIgnoreCase);

            if (transactionDate == DateOnly.MinValue ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(category) ||
                amount == 0)
            {
                Console.WriteLine($"{transactionDate} | {description} | {category} | {amount}");
                continue;
            }
            
            Items.Add(new()
            {
                Amount = amount,
                Description = new ModifiableColumn<string> { OriginalValue = originalDescription, Value = description},
                Category = new ModifiableColumn<string> {OriginalValue = originalCategory, Value = category},
                Date = transactionDate,
                IsDebit = isDebit,
            });
        }
    }
}