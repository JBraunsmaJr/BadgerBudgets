using BadgerBudgets.Models;

namespace BadgerBudgets;

public static class Util
{
    public static string GetName(this ColumnType type)
        => type switch
        {
            ColumnType.TransactionDate => "Transaction Date",
            ColumnType.Amount => "Amount",
            ColumnType.Category => "Category",
            ColumnType.Credit => "Credit",
            ColumnType.Debit => "Debit",
            ColumnType.LineItem => "Description",
            ColumnType.CreditDebitCombined => "Credit & Debit",
            ColumnType.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}