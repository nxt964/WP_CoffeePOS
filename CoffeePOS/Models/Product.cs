namespace CoffeePOS.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool IsStocked
    {
        get; set;
    }
    public int CategoryId
    {
        get; set;
    }

    public bool IsSelected
    {
        get; set;
    }
    public double Price
    {
        get; set;
    }
    public double Cost
    {
        get; set;
    }
    public double Discount
    {
        get; set;
    }
    public string Manufacturer { get; set; } = string.Empty;
    public int Quantity
    {
        get; set;
    }
    public string Type { get; set; } = string.Empty;
    public string Image { get; set; } = "/Assets/StoreLogo.png";
}