using Microsoft.AspNetCore.Components;

namespace BadgerBudgets.Components;

public partial class FileContents : ComponentBase
{
    [Parameter] public string[] Header { get; set; }
    [Parameter] public List<string[]> Rows { get; set; }
}