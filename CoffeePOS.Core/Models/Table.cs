using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;
public class Table
{
    [Key]
    public int Id
    {
        get; set;
    }

    [Required]
    public string TableNumber
    {
        get; set;
    }

    public string Status
    {
        get; set;
    }
}
