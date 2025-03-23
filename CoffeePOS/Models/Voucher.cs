namespace CoffeePOS.Models;

public class Voucher
{
    public int Id
    {
        get; set;
    }
    public string Code
    {
        get; set;
    }
    public override string ToString() => Code;
}