using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;
public class OrderDetail
{
    [Key]
    public int Id
    {
        get; set;
    }

    public int Quantity
    {
        get; set;
    }
    public decimal Price
    {
        get; set;
    }

    [ForeignKey("Order")]
    public int OrderId
    {
        get; set;
    }

    [ForeignKey("Product")]
    public int ProductId
    {
        get; set;
    }
}
