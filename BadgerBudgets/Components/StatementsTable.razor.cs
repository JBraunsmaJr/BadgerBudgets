using BadgerBudgets.Models;
using Microsoft.AspNetCore.Components;
namespace BadgerBudgets.Components;

public partial class StatementsTable : ComponentBase
{
    [Parameter]
    public List<StatementItem> Data { get; set; }

    [Parameter] 
    public string Height { get; set; } = "400px";

    [Parameter] 
    public string Title { get; set; } = "Statements";

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

    protected async Task ManageTransformsForColumn(ColumnType type)
    {
        
    }
}