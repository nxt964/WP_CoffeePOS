using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;
public class Customer
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

    public string Phone
    {
        get; set;
    }
    public bool IsMembership
    {
        get; set;
    }
    public int Points
    {
        get; set;
    }
}
