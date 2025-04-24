namespace CoffeePOS.Models;

public class PaymentMethodDisplay
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