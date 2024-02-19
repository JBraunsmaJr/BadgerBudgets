using BadgerBudgets.Components.Dialogs;
using BadgerBudgets.Models;
using BadgerBudgets.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BadgerBudgets.Components;

public partial class StatementsTable : ComponentBase
{
    [Parameter]
    public List<StatementItem> Data { get; set; }
    
    [Parameter] 
    public string Height { get; set; } = "400px";

    [Parameter] 
    public string Title { get; set; } = "Statements";

    [Parameter] public EventCallback OnShouldUpdate { get; set; }

    [Inject] protected IDialogService DialogService { get; set; }
    [Inject] protected StatementService StatementService { get; set; }

    protected string? SearchString;

    protected bool Filter(StatementItem item)
    {
        if (string.IsNullOrWhiteSpace(SearchString))
            return true;

        if (item.LineItem.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return item.Category.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase) ||
               item.Date.ToString().Contains(SearchString, StringComparison.InvariantCultureIgnoreCase);
    }

    protected async Task ManageTransformsForColumn(ColumnType type, StatementItem statement)
    {
        SourceMaterial? material;

        if (StatementService.SourceMaterials.Count > 1)
        {
            var materialPicker = await DialogService.ShowAsync<SourceMaterialPicker>("Sources");
            var pickerResult = await materialPicker.Result;

            if (pickerResult.Canceled)
                return;

            material = (SourceMaterial?)pickerResult.Data;
        }
        else 
            material = StatementService.SourceMaterials.First().Value;
        
        if (material is null)
            return;
        
        var parameters = new DialogParameters<TransformEditor>
        { 
            { x=>x.SourceMaterial, material },
            { x=>x.StatementItem, statement },
            { x=>x.ColumnType, type }
        };

        var dialog = await DialogService.ShowAsync<TransformEditor>($"Transforms for {type.GetName()}", parameters);
        await dialog.Result;
        StatementService.ApplyTransforms();
        StateHasChanged();
        await OnShouldUpdate.InvokeAsync();
    }
}