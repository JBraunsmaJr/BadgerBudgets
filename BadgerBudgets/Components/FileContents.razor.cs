using BadgerBudgets.Extensions;
using BadgerBudgets.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BadgerBudgets.Components;

public partial class FileContents : ComponentBase
{
    [Parameter] public string[] Header { get; set; }
    [Parameter] public List<string[]> Rows { get; set; }
}