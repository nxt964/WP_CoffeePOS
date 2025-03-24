using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePOS.Core.Models;
public class Product
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
    public double Price
    {
        get; set;
    }
    public string ImageURL
    {
        get; set;
    }
    public bool IsStocked
    {
        get; set;
    }

    [ForeignKey("Category")]
    public int CategoryId
    {
        get; set;
    }
}
