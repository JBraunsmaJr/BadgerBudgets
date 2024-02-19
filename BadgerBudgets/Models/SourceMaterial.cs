using BadgerBudgets.Extensions;

namespace BadgerBudgets.Models;

/// <summary>
/// When ingesting data from multiple sources, headers may vary between them.
/// </summary>
public class SourceMaterial
{
    public string Name { get; set; } = default!;
    public Dictionary<ColumnType, int> Mappings { get; set; } = new();
    public List<ColumnTransform> Transforms { get; set; } = new();
    
    public void SetColumn(ColumnType type, int index)
    {
        Mappings[type] = index;
    }

    public bool HasColumnTransform(ColumnType type, out ColumnTransform? transform)
    {
        transform = Transforms.FirstOrDefault(x => x.Type == type);
        return transform is not null;
    }

    public void UpdateTransform(ColumnTransform transform)
    {
        var index = Transforms.Index(x => x.Type == transform.Type);
        Transforms[index] = transform;
    }
    
    public ColumnType PreviouslyMappedTo(int index) => !Mappings.ContainsValue(index) 
        ? ColumnType.None 
        : Mappings.FirstOrDefault(x => x.Value == index).Key;

    public void RemoveColumn(ColumnType type)
    {
        if (Mappings.ContainsKey(type))
            Mappings.Remove(type);
    }
    
    public string[] ApplyTransforms(string[] row)
    {
        if (row.Length == 0)
            return row;

        HashSet<int> appliedTo = [];
        
        foreach (var transform in Transforms)
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