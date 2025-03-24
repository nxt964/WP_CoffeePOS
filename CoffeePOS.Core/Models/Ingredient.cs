using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePOS.Core.Models;
public class Ingredient
{
    [Key]
    public int Id
    {
        get; set;
    }

    [Required]
    public string Name
    {
        get; set;
    }

    public string Unit
    {
        get; set;
    }
}
