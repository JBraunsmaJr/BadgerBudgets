using BadgerBudgets.Extensions;
using BadgerBudgets.Services;

namespace BadgerBudgets.Models;

/// <summary>
/// When ingesting data from multiple sources, headers may vary between them.
/// </summary>
public class SourceMaterial
{
    public string Name { get; set; } = default!;
    public Dictionary<ColumnType, int> Mappings { get; set; } = new();
    public Dictionary<ColumnType, List<ColumnTransform>> Transforms = new();

    public void SetColumn(ColumnType type, int index)
    {
        Mappings[type] = index;
    }

    public void UpdateTransform(int index, ColumnTransform transform)
    {
        Transforms[transform.Type][index] = transform;
    }

    public void AddTransform(ColumnTransform transform)
    {
        if (!Transforms.ContainsKey(transform.Type))
            Transforms.Add(transform.Type, new()
            {
                transform
            });
        else
            Transforms[transform.Type].Add(transform);
    }
    
    public ColumnType PreviouslyMappedTo(int index) => !Mappings.ContainsValue(index) 
        ? ColumnType.None 
        : Mappings.FirstOrDefault(x => x.Value == index).Key;

    public void RemoveColumn(ColumnType type)
    {
        if (Mappings.ContainsKey(type))
            Mappings.Remove(type);
    }

    public void ApplyTransforms()
    {
        foreach (var item in StatementService.Items)
        {
            foreach (var transformArea in Transforms)
            {
                Console.WriteLine($"{transformArea.Key} - checking");
                
                foreach (var transform in transformArea.Value)
                {
                    // Transform is conditional on another column
                    if (transform.ColumnCondition is not null)
                    {
                        var incoming = item.GetColumnValue(transform.ColumnCondition.ColumnType);
                        Console.WriteLine($"Incoming: {incoming} | {transform}");
                        if (!transform.ColumnCondition.IsValid(item.GetColumnValue(transform.ColumnCondition.ColumnType)))
                        {
                            Console.WriteLine($"Incoming {incoming} | {transform} ------ failed condition check");
                            continue;
                        }

                        item.UpdateValue(transform.Type, transform.DestinationValue);
                    }
                    else if (transform.IsValid(item.GetColumnValue(transform.Type)))
                    {
                        Console.WriteLine($"Transform: {transform} --- passed");
                        item.UpdateValue(transform.Type, transform.DestinationValue);
                    }
                }
            }   
        }
    }
    
    public string[] ApplyTransforms(string[] row)
    {
        if (row.Length == 0)
            return row;

        HashSet<int> appliedTo = [];
        
        foreach (var transform in Transforms.SelectMany(x=>x.Value))
        {
            if (!Mappings.ContainsKey(transform.Type))
                continue;
            
            var columnIndex = Mappings[transform.Type];
            
            // Avoid applying transform to something that's already been transformed
            if (appliedTo.Contains(columnIndex))
                continue;

            // Avoid out of bounds
            if (columnIndex >= row.Length)
                continue;
            
            // Transform is conditional on another column
            if (transform.ColumnCondition is not null)
            {
                if (!Mappings.ContainsKey(transform.ColumnCondition.ColumnType))
                    continue;
                
                var conditionalIndex = Mappings[transform.ColumnCondition.ColumnType];

                if (conditionalIndex >= row.Length)
                    continue;

                if (!transform.ColumnCondition.IsValid(row[conditionalIndex]) ||
                    !transform.IsValid(row[columnIndex])) continue;
                
                row[columnIndex] = transform.DestinationValue;
                appliedTo.Add(columnIndex);
            }
            else if (transform.IsValid(row[columnIndex]))
            {
                row[columnIndex] = transform.DestinationValue;
                appliedTo.Add(columnIndex);
            }
        }

        return row;
    }
}