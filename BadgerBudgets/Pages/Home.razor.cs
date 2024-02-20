﻿using System.Globalization;
using BadgerBudgets.Models;
using BadgerBudgets.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using MudBlazor.Extensions;

namespace BadgerBudgets.Pages;

public partial class Home : ComponentBase
{
    #region Injects
    [Inject] protected StatementService StatementService { get; set; }
    [Inject] protected ISnackbar SnackbarService { get; set; }
    #endregion

    #region Visualizations
    public bool ShowVisualizations { get; set; } = true;
    private int _selectedCategoryIndex;

    #endregion
    
    #region Filtering
    private DateRange? _filterDateRange;
    private NumberRange<double>? _filterAmountRange;
    private string selectedCategory { get; set; } = "Nothing selected";
    private IEnumerable<string> selectedCategories;
    private List<StatementItem> _items = new();

    #endregion
    
    #region Drag and drop files    
    private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full z-10";
    private string _dragClass = DefaultDragClass;
    private void SetDragClass() => _dragClass = $"{DefaultDragClass} mud-border-primary";
    private void ClearDragClass() => _dragClass = DefaultDragClass;
    #endregion
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await StatementService.LoadFromStorage();

        _items = StatementService.Items;
        selectedCategories = StatementService.Items.Select(x => x.Category.Value).Distinct().ToHashSet();
    }
    
    private IQueryable<StatementItem> FilterQuery
    {
        get
        {
            var query = StatementService.Items.AsQueryable();

            // Date based filtering
            if (_filterDateRange is not null)
            {
                if (_filterDateRange.Start is not null && _filterDateRange.End is not null)
                {
                    var startDate = DateOnly.FromDateTime(_filterDateRange.Start.Value);
                    var endDate = DateOnly.FromDateTime(_filterDateRange.End.Value);

                    query = query.Where(x => x.Date >= startDate && x.Date <= endDate);
                }
                else if (_filterDateRange.Start is not null)
                {
                    var startDate = DateOnly.FromDateTime(_filterDateRange.Start.Value);
                    query = query.Where(x => x.Date >= startDate);
                }
                else if (_filterDateRange.End is not null)
                {
                    var endDate = DateOnly.FromDateTime(_filterDateRange.End.Value);
                    query = query.Where(x => x.Date <= endDate);
                }
            }

            // Amount based filtering
            if (_filterAmountRange is not null)
            {
                var min = Math.Min(_filterAmountRange.Value.Min, _filterAmountRange.Value.Max);
                var max = Math.Max(_filterAmountRange.Value.Min, _filterAmountRange.Value.Max);

                query = query.Where(x => x.Amount >= min && x.Amount <= max);
            }
            
            // Category based filtering
            if (selectedCategories.Any())
                query = query.Where(x => selectedCategories.Contains(x.Category.Value));
            
            return query;
        }
    }

    async Task OnFileUpload(string sourceName, IBrowserFile uploadedFile)
    {
        try
        {
            using MemoryStream memStream = new();
            await uploadedFile.OpenReadStream().CopyToAsync(memStream);
            using TextReader reader = new StreamReader(memStream);
            memStream.Position = 0;

            var hadItems = StatementService.Items.Any();
            await StatementService.ParseFile(sourceName, reader);
            SnackbarService.Add($"Import Successful. {StatementService.Items.Count} Total Records", Severity.Success);
            
            if(!hadItems)
                selectedCategories = StatementService.Items.Select(x => x.Category.Value).Distinct().ToHashSet();
            else if (selectedCategories is null)
                selectedCategories = new HashSet<string>();
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    private void SetDateRangeToLastThreeMonths()
    {
        var start = DateTime.Now.AddMonths(-3).StartOfMonth(CultureInfo.CurrentCulture);
        var endOfMonth = DateTime.Now.EndOfMonth(CultureInfo.CurrentCulture);

        _filterDateRange = new DateRange(start, endOfMonth);
        StateHasChanged();
    }

    private void SetDateRangeToLastMonth()
    {
        var start = DateTime.Now.AddMonths(-1).StartOfMonth(CultureInfo.CurrentCulture);
        var end = start.EndOfMonth(CultureInfo.CurrentCulture);

        _filterDateRange = new DateRange(start, end);
        StateHasChanged();
    }

    private void SetDateRangeToCurrentMonth()
    {
        var start = DateTime.Now.StartOfMonth(CultureInfo.CurrentCulture);
        var end = DateTime.Now.EndOfMonth(CultureInfo.CurrentCulture);

        _filterDateRange = new DateRange(start, end);
        StateHasChanged();
    }

    private string GetMultiSelectionCategory(List<string> selectedValues) => 
        $"{selectedValues.Count} {(selectedValues.Count > 1 ? "categories have" : "category has")} been selected";
}