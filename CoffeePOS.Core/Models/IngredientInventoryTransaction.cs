using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;

public class IngredientInventoryTransaction
{
    public int Id
    {
        get; set;
    }
    public int IngredientId
    {
        get; set;
    }
    public long Timestamp
    {
        get; set;
    } // Unix timestamp in milliseconds
    public int Quantity
    {
        get; set;
    }
    public string Unit { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty; // "IMPORT" or "EXPORT"
    public double UnitPrice
    {
        get; set;
    }
}