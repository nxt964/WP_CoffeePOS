using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePOS.Core.Models;
public class Category
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

    public string Description
    {
        get; set;
    }
}
