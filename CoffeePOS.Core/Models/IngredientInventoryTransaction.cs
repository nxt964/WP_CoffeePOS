using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeePOS.Core.Models;
public class IngredientInventoryTransaction
{
    [Key]
    public int Id
    {
        get; set;
    }

    [ForeignKey("Ingredient")]
    public int IngredientId
    {
        get; set;
    }

    public decimal QuantityChange
    {
        get; set;
    }
    public DateTime TransactionDate
    {
        get; set;
    }
    public string TransactionType
    {
        get; set;
    }
}
