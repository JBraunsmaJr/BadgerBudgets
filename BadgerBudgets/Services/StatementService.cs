using BadgerBudgets.Extensions;
using BadgerBudgets.Models;
using Blazored.LocalStorage;
using Newtonsoft.Json;

namespace BadgerBudgets.Services;

public class StatementService
{
    public static List<StatementItem> Items { get; set; } = new();
    private static Dictionary<ColumnType, int> _columnMappings = new();
    public static bool HasContent => Items.Count != 0;
    public static bool HasMappings => _columnMappings.Count > 0;
    private const string MappingsKey = "ColumnMappings";

    private readonly ILocalStorageService _storage;
    private readonly ILogger<StatementService> _logger;

    public StatementService(ILocalStorageService storage, ILogger<StatementService> logger)
    {
        _storage = storage;
        _logger = logger;
        Task.Run(LoadFromStorage);
    }

    async Task LoadFromStorage()
    {
        if (!await _storage.ContainKeyAsync(MappingsKey))
            return;

        try
        {
            var json = await _storage.GetItemAsStringAsync(MappingsKey);
            
            if(!string.IsNullOrWhiteSpace(json))
                _columnMappings = JsonConvert.DeserializeObject<Dictionary<ColumnType, int>>(json)!;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load from storage: {Message}", ex);
        }
    }

    public async Task SetMappings(Dictionary<ColumnType, int> mappings)
    {
        _columnMappings = mappings;
        await SaveToLocal();
    }

    public async Task RemoveMapping(ColumnType type)
    {
        if (!_columnMappings.ContainsKey(type))
            return;
        _columnMappings.Remove(type);
        await Task.Run(SaveToLocal);
    }

    public async Task RemoveMapping(int index)
    {
        if (!_columnMappings.ContainsValue(index)) return;

        var valueIndex = _columnMappings.Values.Index(x => x == index);
        var keyIndex = _columnMappings.Keys.ToArray()[valueIndex];
        _columnMappings.Remove(keyIndex);
        await Task.Run(SaveToLocal);
    }

    public async Task UpdateMapping(ColumnType type, int columnIndex)
    {
        _columnMappings[type] = columnIndex;
        await Task.Run(SaveToLocal);
    }

    async Task SaveToLocal()
    {
        await _storage.SetItemAsStringAsync(MappingsKey, JsonConvert.SerializeObject(_columnMappings));
    }

    public string SerializeMappings()
    {
        return JsonConvert.SerializeObject(_columnMappings);
    }

    public Dictionary<string, List<double>> GetAmountsByCategory(bool ignoreCredit = true)
    {
        return Items.GroupBy(x => x.Category)
            .Where(x => !StatementItem.CreditCategories.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Select(y => y.Amount).ToList());
    }

    public void ParseFile(string[] lines, char delimiter = ',')
    {
        foreach (var line in lines)
        {
            var parts = line.Split(delimiter);
            if (parts.Length < 5)
                continue;

            DateOnly.TryParse(parts[_columnMappings[ColumnType.TransactionDate]], out var transactionDate);
            double.TryParse(parts[_columnMappings[ColumnType.Amount]], out var amount);
            var description = parts[_columnMappings[ColumnType.LineItem]];
            var category = parts[_columnMappings[ColumnType.Category]];

            var isDebit = true;
            if (_columnMappings.TryGetValue(ColumnType.Debit, out var mapping))
            {
                isDebit = parts[mapping]
                    .Contains("debit", StringComparison.InvariantCultureIgnoreCase);
            }
            else if (_columnMappings.TryGetValue(ColumnType.CreditDebitCombined, out var columnMapping))
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
                IsDebit = isDebit
            });
        }
    }
}