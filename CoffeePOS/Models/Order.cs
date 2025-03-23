namespace CoffeePOS.Models;

public class Order
{
    public string OrderId
    {
        get; set;
    }
    public string Date
    {
        get; set;
    }
    public string CustomerId
    {
        get; set;
    }
    public decimal TotalPrice
    {
        get; set;
    }
    public string VoucherId
    {
        get; set;
    }
    public string PaymentMethodId
    {
        get; set;
    }
    public string ServiceTypeId
    {
        get; set;
    }
    public string TableId
    {
        get; set;
    }
}