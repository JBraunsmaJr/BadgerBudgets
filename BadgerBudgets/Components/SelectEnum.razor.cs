using Microsoft.AspNetCore.Components;

namespace BadgerBudgets.Components;

public partial class SelectEnum<TEnum> : ComponentBase
    where TEnum : struct, Enum
{
    [Parameter] public string Label { get; set; }
    [Parameter] public EventCallback<TEnum> OnValueChanged { get; set; }

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

        if (_cache.Count > 0) return;
        
        var values = Enum.GetValues<TEnum>();
        var names = Enum.GetNames<TEnum>();

        for (var i = 0; i < values.Length; i++)
            _cache.Add(values[i], names[i]);
    }
}