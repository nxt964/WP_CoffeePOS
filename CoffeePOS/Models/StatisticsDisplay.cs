namespace CoffeePOS.Models;

public class OrderDisplayModel
{
    public int Id
    {
        get; set;
    }
    public string OrderDate
    {
        get; set;
    }
    public double TotalAmount
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
    public string CustomerName
    {
        get; set;
    }
    public string PaymentMethodName
    {
        get; set;
    }
    public string ServiceTypeName
    {
        get; set;
    }

    // Additional property for expandable row details
    public List<OrderItemModel> OrderItems { get; set; } = new List<OrderItemModel>();
}

public class OrderItemModel
{
    public int ProductId
    {
        get; set;
    }
    public string ProductName
    {
        get; set;
    }
    public int Quantity
    {
        get; set;
    }
    public double Price
    {
        get; set;
    }
}

public class ProductSalesViewModel
{
    public int ProductId
    {
        get; set;
    }
    public string ProductName
    {
        get; set;
    }
    public string CategoryName
    {
        get; set;
    }
    public string Image
    {
        get; set;
    }
    public int QuantitySold
    {
        get; set;
    }
    public double TotalSales
    {
        get; set;
    }
}

public class IngredientCostViewModel
{
    public int IngredientId
    {
        get; set;
    }
    public string IngredientName
    {
        get; set;
    }
    public int TransactionCount
    {
        get; set;
    }
    public double TotalQuantity
    {
        get; set;
    }
    public string Unit
    {
        get; set;
    }
    public double TotalCost
    {
        get; set;
    }
}

public class ServiceTypeViewModel
{
    public int ServiceTypeId
    {
        get; set;
    }
    public string ServiceTypeName
    {
        get; set;
    }
    public int OrderCount
    {
        get; set;
    }
    public double TotalSales
    {
        get; set;
    }
    public double Percentage
    {
        get; set;
    }
}

public class PaymentMethodViewModel
{
    public int PaymentMethodId
    {
        get; set;
    }
    public string PaymentMethodName
    {
        get; set;
    }
    public int OrderCount
    {
        get; set;
    }
    public double TotalSales
    {
        get; set;
    }
    public double Percentage
    {
        get; set;
    }
}

public class CategoryDistributionViewModel
{
    public int CategoryId
    {
        get; set;
    }
    public string CategoryName
    {
        get; set;
    }
    public double TotalSales
    {
        get; set;
    }
    public double Percentage
    {
        get; set;
    }
}

public class CustomerDisplayModel
{
    public int CustomerId
    {
        get; set;
    }
    public string CustomerName
    {
        get; set;
    }
    public string Phone
    {
        get; set;
    }
    public string MembershipStatus
    {
        get; set;
    }
    public int Points
    {
        get; set;
    }
    public int OrderCount
    {
        get; set;
    }
    public double TotalSpent
    {
        get; set;
    }
}
