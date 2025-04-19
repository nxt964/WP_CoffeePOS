using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;

public class Ingredient
{
    public int Id
    {
        get; set;
    }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity
    {
        get; set;
    }
    public int Threshold
    {
        get; set;
    } // Alert threshold for low stock
}