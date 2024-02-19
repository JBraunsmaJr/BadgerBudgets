using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BadgerBudgets.Models;
using Microsoft.AspNetCore.Components;

namespace BadgerBudgets.Components;

public partial class SelectEnum<TEnum> : ComponentBase
    where TEnum : struct, Enum
{
    [Parameter] public string Label { get; set; }
    [Parameter] public EventCallback<TEnum> OnValueChanged { get; set; }
    [Parameter] public TEnum? SelectedValue { get; set; }

    private TEnum _value;

    public TEnum Value
    {
        get => _value;
        set
        {
            _value = value;
            OnValueChanged.InvokeAsync(_value).GetAwaiter().GetResult();
        }
    }
    private static Dictionary<TEnum, string> _cache = new();
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (SelectedValue is not null)
            _value = SelectedValue.Value;
        
        if (_cache.Count > 0) return;
        
        var values = Enum.GetValues<TEnum>();

        var names = new List<string>();
        foreach (var enumVal in values)
        {
            var enumName = Enum.GetName(enumVal);
            var memberInfos = typeof(ColumnType).GetMember(enumName!);
            var enumValueMemberInfo = memberInfos.FirstOrDefault(x => x.DeclaringType == typeof(ColumnType));

            if (enumValueMemberInfo is null)
            {
                names.Add(enumName!);
                continue;
            }
            
            var display = enumValueMemberInfo.GetCustomAttribute<DisplayAttribute>();
            names.Add(display is null ? enumName! : display.Name!);
        }

        for (var i = 0; i < values.Length; i++)
            _cache.Add(values[i], names[i]);
    }
}