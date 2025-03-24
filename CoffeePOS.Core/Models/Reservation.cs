using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Models;
public class Reservation
{
    [Key]
    public int Id
    {
        get; set;
    }

    [ForeignKey("Customer")]
    public int CustomerId
    {
        get; set;
    }

    [ForeignKey("Table")]
    public int TableId
    {
        get; set;
    }

    public DateTime ReservationDate
    {
        get; set;
    }
    public TimeSpan StartTime
    {
        get; set;
    }
    public TimeSpan EndTime
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
}
