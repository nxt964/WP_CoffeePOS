namespace CoffeePOS.Models;

public class PaymentMethod
{
    public int Id
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public override string ToString() => Name;
}