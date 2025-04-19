using System;

namespace CoffeePOS.Models;

public class CustomerDisplay
{
    public int Id
    {
        get; set;
    }
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