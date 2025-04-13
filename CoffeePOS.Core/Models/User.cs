using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CoffeePOS.Core.Models;
public class User
{
    [Key]
    public int Id
    {
        get; set;
    }

    [Required]
    public string Username
    {
        get; set;
    }

    [Required]
    public string Password
    {
        get; set;
    }

    public DateTime? ExpireAt
    {
        get; set;
    }
}
