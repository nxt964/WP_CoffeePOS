using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;
public class Voucher
{
    [Key]
    public int Id
    {
        get; set;
    }

    [Required]
    public string Code
    {
        get; set;
    }

    public decimal DiscountPercentage
    {
        get; set;
    }
}
