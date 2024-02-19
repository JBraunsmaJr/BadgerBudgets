using System.ComponentModel.DataAnnotations;

namespace BadgerBudgets.Models;

public enum ColumnType
{
    None,
    
    [Display(Name = "Transaction Date")]
    TransactionDate,
    
    [Display(Name = "Description")]
    LineItem,
    
    Amount,
    Debit,
    Credit,
    Category,
    
    [Display(Name = "Credit & Debit")]
    CreditDebitCombined
}