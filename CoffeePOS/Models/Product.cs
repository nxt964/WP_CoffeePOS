namespace CoffeePOS.Models;

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
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