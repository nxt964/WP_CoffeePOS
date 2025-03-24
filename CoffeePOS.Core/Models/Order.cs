using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;
public class Order
{
    [Key]
    public int Id
    {
        get; set;
    }

    [ForeignKey("Customer")]
    public int? CustomerId
    {
        get; set;
    }

    public DateTime OrderDate
    {
        get; set;
    }
    public DateTime? PaymentDate
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
    public decimal TotalAmount
    {
        get; set;
    }

    [ForeignKey("Table")]
    public int? TableId
    {
        get; set;
    }

    [ForeignKey("Voucher")]
    public int? VoucherId
    {
        get; set;
    }

    [ForeignKey("PaymentMethod")]
    public int? PaymentMethodId
    {
        get; set;
    }

    [ForeignKey("ServiceType")]
    public int? ServiceTypeId
    {
        get; set;
    }
}
