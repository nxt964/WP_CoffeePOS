using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeePOS.Core.Models;
public class ProductIngredient
{
    [Key]
    public int Id
    {
        get; set;
    }

    [ForeignKey("Product")]
    public int ProductId
    {
        get; set;
    }

    [ForeignKey("Ingredient")]
    public int IngredientId
    {
        get; set;
    }

    public decimal QuantityUsed
    {
        get; set;
    }
}
