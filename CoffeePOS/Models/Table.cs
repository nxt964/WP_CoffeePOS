namespace CoffeePOS.Models;

public class Table
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