﻿using BadgerBudgets.Components.Dialogs;
using BadgerBudgets.Extensions;
using BadgerBudgets.Models;
using BadgerBudgets.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BadgerBudgets.Components;

public enum SourceMaterialEditorStage
{
    UploadStatement,
    ProvideName,
    ProvideColumnMappings
}

public partial class SourceMaterialEditor : ComponentBase
{
    [Inject] protected ILogger<SourceMaterialEditor> Logger { get; set; }
    [Inject] protected StatementService StatementService { get; set; }
    [Inject] protected ISnackbar Snackbar { get; set; }
    [Inject] protected NavigationManager NavManager { get; set; }
    [Inject] protected IDialogService DialogService { get; set; }
    [Parameter] public SourceMaterial? SourceMaterial { get; set; }

    private SourceMaterialEditorStage _currentStage = SourceMaterialEditorStage.ProvideName;
    private SourceMaterial _source;
    private string? _materialSourceName;
    private bool _isValidForm;
    private string[] _formErrors = Array.Empty<string>();
    private MudForm _form;
    private Dictionary<string, MudSelect<ColumnType>> _mappingSelects = new();

    public string[] HeaderRow { get; private set; } = Array.Empty<string>();
    public List<string[]> Rows { get; private set; } = new();

    private static Dictionary<string, SourceMaterialEditorStage> stages = new()
    {
        ["Provide Name"] = SourceMaterialEditorStage.ProvideName,
        ["Upload Statement"] = SourceMaterialEditorStage.UploadStatement,
        ["Map Columns"] = SourceMaterialEditorStage.ProvideColumnMappings
    };
    
    protected MudSelect<ColumnType> Ref
    {
        set
        {
            var count = _mappingSelects.Count;

            if (count >= HeaderRow.Length)
                return;
            
            var headerText = HeaderRow[count];
            _mappingSelects.Add(headerText, value);
        }
    }
    
    private HashSet<ColumnType> _remainingColumnTypes =
    [
        ColumnType.TransactionDate,
        ColumnType.Amount,
        ColumnType.LineItem,
        ColumnType.Category,
        ColumnType.Credit,
        ColumnType.Debit,
        ColumnType.CreditDebitCombined
    ];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (SourceMaterial is not null)
            _source = SourceMaterial;
        else _source = new();

        _materialSourceName = _source.Name;
    }

    private void OnSubmitName()
    {
        _source.Name = _materialSourceName!;
        _currentStage = SourceMaterialEditorStage.UploadStatement;
        StateHasChanged();
    }

    private async Task OnCreateMappingClick()
    {
        var missingRequiredMaps = new List<string>();
        if (!_source.Mappings.ContainsKey(ColumnType.TransactionDate))
            missingRequiredMaps.Add(ColumnType.TransactionDate.GetName());
        if (!_source.Mappings.ContainsKey(ColumnType.Amount))
            missingRequiredMaps.Add(ColumnType.Amount.GetName());
        if (!_source.Mappings.ContainsKey(ColumnType.LineItem))
            missingRequiredMaps.Add(ColumnType.LineItem.GetName());

        if (missingRequiredMaps.Count != 0)
        {
            Snackbar.Add($"Please map the following: {string.Join(", ", missingRequiredMaps)}", 
                Severity.Error);
            return;
        }

        try
        {
            StatementService.SourceMaterials[_source.Name] = _source;
            await StatementService.Save();
            Snackbar.Add($"{_source.Name} successfully saved!", Severity.Success);
            NavManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            Logger.LogError("An error occurred while saving: {Error}", ex);
            Snackbar.Add("Was unable to save", Severity.Error);
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        foreach (var pair in _mappingSelects)
            pair.Value.ValueChanged = EventCallback.Factory.Create<ColumnType>(this, ()=>OnColumnMappingChanged(pair.Key));
    }

    protected async Task EditTransform(ColumnTransform transform)
    {
        var parameters = new DialogParameters<TransformDialog>();
        parameters.Add(x => x.Transform, transform);
        parameters.Add(x => x.ColumnType, transform.Type);

        var dialog = await DialogService.ShowAsync<TransformDialog>("Edit Transform", parameters);
        var result = await dialog.Result;

        if (result.Canceled)
            return;
        
        transform = (ColumnTransform)result.Data;
        _source.UpdateTransform(transform);
    }

    protected async Task CreateTransform(ColumnType type)
    {
        var parameters = new DialogParameters<TransformDialog>();
        parameters.Add(x => x.ColumnType, type);

        var dialog = await DialogService.ShowAsync<TransformDialog>("New Transform", parameters);
        var result = await dialog.Result;

        if (result.Canceled)
            return;

        var transform = (ColumnTransform)result.Data;
        _source.Transforms.Add(transform);
    }
    
    private void OnColumnMappingChanged(string headerText)
    {
        if (!_mappingSelects.ContainsKey(headerText))
        {
            Logger.LogError("Mapping selects does not have {Value}", headerText);
            return;
        }
        
        var selected = _mappingSelects[headerText].Value;
        var headerIndex = HeaderRow.Index(x => x == headerText);
        var previousCol = _source.PreviouslyMappedTo(headerIndex);

        // We have to insert our old column back since we're overwriting it.
        if (previousCol != ColumnType.None)
            _source.Mappings.Remove(previousCol);
        
        _source.SetColumn(selected, headerIndex);

        StateHasChanged();
    }
    
    protected async Task OnFileUpload(IBrowserFile uploadedFile)
    {
        try
        {
            using MemoryStream memStream = new();
            await uploadedFile.OpenReadStream().CopyToAsync(memStream);
            using TextReader reader = new StreamReader(memStream);
            memStream.Position = 0;
            var contents = await reader.ReadToEndAsync();

            var lines = contents.Split('\n');
            HeaderRow = lines[0].Split(',');

            foreach (var line in lines[1..])
            {
                if (string.IsNullOrWhiteSpace(line.Trim()))
                    continue;
                var parts = line.Split(',');
                Rows.Add(parts);
            }

            // Only go to next stage once valid content is provided
            if(HeaderRow.Length > 0 && Rows.Count > 0)
                _currentStage = SourceMaterialEditorStage.ProvideColumnMappings;
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError("An error occurred while processing {File}. Exception: {Exception}",
                uploadedFile.Name, ex);
        }
    }
}