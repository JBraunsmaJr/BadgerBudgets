using BadgerBudgets.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BadgerBudgets.Components.Dialogs;

public partial class TransformDialog : ComponentBase
{
    [CascadingParameter] public MudDialogInstance DialogInstance { get; set; }
    [Parameter] public ColumnTransform? Transform { get; set; }
    [Parameter] public ColumnType ColumnType { get; set; }

    private string? _sourceValue;
    private string? _sourceDestinationValue;
    private TransformCondition _sourceCondition = TransformCondition.Contains;
    private ConditionalColumnTransform? _conditionalTransform;
    private bool _useConditional;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Transform ??= new();

        _sourceValue = Transform.SourceValue;
        _sourceCondition = Transform.Condition;
        _sourceDestinationValue = Transform.DestinationValue;
        _useConditional = Transform.ColumnCondition is not null;
        _conditionalTransform = Transform.ColumnCondition;
    }

    private void Submit()
    {
        if (Transform is null)
            return;
        
        Transform!.SourceValue = _sourceValue!;
        Transform.Type = ColumnType;
        Transform.SourceValue = _sourceDestinationValue!;
        Transform.Condition = _sourceCondition;
        Transform.ColumnCondition = _conditionalTransform;
        
        DialogInstance.Close(DialogResult.Ok(Transform));
    }

    public void Cancel() => DialogInstance.Cancel();
}