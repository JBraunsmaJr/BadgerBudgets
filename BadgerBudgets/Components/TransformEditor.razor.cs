using System.Globalization;
using BadgerBudgets.Components.Dialogs;
using BadgerBudgets.Extensions;
using BadgerBudgets.Models;
using BadgerBudgets.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BadgerBudgets.Components;

public partial class TransformEditor : ComponentBase
{
    #region Parameters
    [Parameter] public SourceMaterial SourceMaterial { get; set; }
    [Parameter] public ColumnType ColumnType { get; set; }
    [Parameter] public StatementItem StatementItem { get; set; }
    [CascadingParameter] private MudDialogInstance DialogInstance { get; set; }
    #endregion
    
    #region Injects
    [Inject] private StatementService StatementService { get; set; }
    [Inject] private ISnackbar Snackbar { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    #endregion
    
    async Task OnUpdate(ColumnTransform transform)
    {
        var index = SourceMaterial.Transforms[ColumnType].Index(x => x.Equals(transform));
        var parameters = new DialogParameters<TransformDialog>();
        parameters.Add(x=>x.ColumnType, ColumnType);
        parameters.Add(x => x.Transform, transform);

        var dialog = await DialogService.ShowAsync<TransformDialog>("Update Dialog", parameters);
        var result = await dialog.Result;

        if (result.Canceled)
            return;
        
        var newData = (ColumnTransform)result.Data;
        
        transform.Condition = newData.Condition;
        transform.DestinationValue = newData.DestinationValue;
        transform.SourceValue = newData.SourceValue;
        transform.ColumnCondition = newData.ColumnCondition;
        SourceMaterial.UpdateTransform(index, transform);
        await StatementService.Save();
        
        Console.WriteLine("I HAVE: " + SourceMaterial.Transforms[ColumnType][index].ToString());
        Snackbar.Add($"{transform.Type.GetName()} saved", Severity.Success);
    }

    async Task OnDelete(ColumnTransform transform)
    {
        SourceMaterial.Transforms[ColumnType].Remove(transform);
        await StatementService.Save();
        Snackbar.Add($"{transform.Type.GetName()} deleted", Severity.Error);
    }

    async Task OnCreate()
    {
        var parameters = new DialogParameters<TransformDialog>();
        parameters.Add(x => x.ColumnType, ColumnType);
        parameters.Add(x=>x.IncomingValue, StatementItem.GetColumnValue(ColumnType));
        
        var dialog = await DialogService.ShowAsync<TransformDialog>("New Transform", parameters);
        var result = await dialog.Result;

        if (result.Canceled)
            return;
        
        SourceMaterial.AddTransform((ColumnTransform)result.Data);
        await StatementService.Save();
    }
}