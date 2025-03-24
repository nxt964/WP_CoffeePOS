using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CoffeePOS.Core.Models;
public class Employee
{
    [Key]
    public int Id
    {
        get; set;
    }

    [Required]
    public string EmployeeName
    {
        get; set;
    }

    public string Email
    {
        get; set;
    }
    public string Phone
    {
        get; set;
    }
    public double Salary
    {
        get; set;
    }
}
