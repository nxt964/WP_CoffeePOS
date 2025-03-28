namespace CoffeePOS.Models;

public class OrderDisplay
{
    public int Id
    {
        get; set;
    }
    public DateTimeOffset OrderDate
    {
        get; set;
    }
    public string CustomerName
    {
        get; set;
    }
    public decimal TotalAmount
    {
        get; set;
    }
    public string ServiceTypeName
    {
        get; set;
    }
}