namespace BadgerBudgets.Models;

public class StatementItem
{
    public DateOnly Date { get; set; }
    public string LineItem { get; set; }
    public double Amount { get; set; }
    public bool IsDebit { get; set; }
    public bool IsCredit => !IsDebit;
    public string Category { get; set; }
}