namespace CoffeePOS.ViewModels;

public class OrderDisplay
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
}