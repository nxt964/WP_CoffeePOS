using System.Collections.ObjectModel;
using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace CoffeePOS.ViewModels;

public partial class StatisticsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;

    // Date filters
    [ObservableProperty]
    private DateTimeOffset _startDate;

    [ObservableProperty]
    private DateTimeOffset _endDate;

    [ObservableProperty]
    private string _orderStatusFilter = "All";

    // Summary statistics
    [ObservableProperty]
    private double _totalRevenue;

    [ObservableProperty]
    private double _totalCosts;

    [ObservableProperty]
    private double _netProfit;

    [ObservableProperty]
    private double _profitMargin;

    [ObservableProperty]
    private double _averageOrderValue;

    [ObservableProperty]
    private int _orderCount;

    [ObservableProperty]
    private int _totalProductsSold;

    [ObservableProperty]
    private int _ingredientTransactionCount;

    // Data collections
    [ObservableProperty]
    private ObservableCollection<OrderDisplayModel> _recentOrders = new();

    [ObservableProperty]
    private ObservableCollection<ProductSalesViewModel> _topProducts = new();

    [ObservableProperty]
    private ObservableCollection<IngredientCostViewModel> _ingredientCosts = new();

    [ObservableProperty]
    private ObservableCollection<ServiceTypeViewModel> _serviceTypeDistribution = new();

    [ObservableProperty]
    private ObservableCollection<PaymentMethodViewModel> _paymentMethodDistribution = new();

    // Constructor
    public StatisticsViewModel(IDao dao)
    {
        _dao = dao;

        // Initialize date range to the current month
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        EndDate = StartDate.AddMonths(1).AddDays(-1);
    }

    public async void OnNavigatedTo(object parameter)
    {
        Debug.WriteLine("[DEBUG] StatisticsViewModel.OnNavigatedTo: Loading statistics");
        await LoadStatisticsAsync();
    }

    public void OnNavigatedFrom()
    {
        // Nothing to clean up
    }

    [RelayCommand]
    private async Task LoadStatistics()
    {
        Debug.WriteLine("[DEBUG] StatisticsViewModel.LoadStatistics: Reloading all statistics");
        await LoadStatisticsAsync();
    }

    [RelayCommand]
    private Task FilterOrders()
    {
        Debug.WriteLine($"[DEBUG] StatisticsViewModel.FilterOrders: Filtering orders by status: {OrderStatusFilter}");
        return LoadOrdersAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            await Task.WhenAll(
                LoadSummaryStatisticsAsync(),
                LoadOrdersAsync(),
                LoadTopProductsAsync(),
                LoadIngredientCostsAsync(),
                LoadDistributionAsync()
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] Error loading statistics: {ex.Message}");
        }
    }

    private async Task LoadSummaryStatisticsAsync()
    {
        // Get total revenue from completed orders
        var allOrders = await _dao.Orders.GetAll();
        var filteredOrders = allOrders.Where(o =>
            DateTime.TryParse(o.OrderDate.ToString(), out var orderDate) &&
            orderDate >= StartDate.DateTime &&
            orderDate <= EndDate.DateTime &&
            o.Status == "Completed").ToList();

        // Calculate total revenue
        TotalRevenue = (double)filteredOrders.Sum(o => o.TotalAmount);
        OrderCount = filteredOrders.Count;

        // Get order details to count products sold
        var allOrderDetails = await _dao.OrderDetails.GetAll();
        var relevantOrderDetails = allOrderDetails.Where(od =>
            filteredOrders.Any(o => o.Id == od.OrderId)).ToList();

        TotalProductsSold = relevantOrderDetails.Sum(od => od.Quantity);

        // Get ingredient costs
        var allIngredientTransactions = await _dao.IngredientInventoryTransactions.GetAll();
        var filteredTransactions = allIngredientTransactions.Where(t =>
            t.TransactionType == "IMPORT" &&
            DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp) >= StartDate &&
            DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp) <= EndDate).ToList();

        TotalCosts = filteredTransactions.Sum(t => t.Quantity * t.UnitPrice);
        IngredientTransactionCount = filteredTransactions.Count;

        // Calculate profit and margins
        NetProfit = TotalRevenue - TotalCosts;
        ProfitMargin = TotalRevenue > 0 ? NetProfit / TotalRevenue : 0;

        // Calculate average order value
        AverageOrderValue = OrderCount > 0 ? TotalRevenue / OrderCount : 0;
    }

    private async Task LoadOrdersAsync()
    {
        RecentOrders.Clear();

        var allOrders = await _dao.Orders.GetAll();
        var customers = await _dao.Customers.GetAll();
        var paymentMethods = await _dao.PaymentMethods.GetAll();
        var serviceTypes = await _dao.ServiceTypes.GetAll();

        var filteredOrders = allOrders.Where(o =>
            DateTime.TryParse(o.OrderDate.ToString(), out var orderDate) &&
            orderDate >= StartDate.DateTime &&
            orderDate <= EndDate.DateTime &&
            (OrderStatusFilter == "All" || o.Status == OrderStatusFilter))
            .OrderByDescending(o => o.OrderDate)
            .Take(100)  // Limit to most recent 100 orders
            .ToList();

        foreach (var order in filteredOrders)
        {
            var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
            var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.Id == order.PaymentMethodId);
            var serviceType = serviceTypes.FirstOrDefault(st => st.Id == order.ServiceTypeId);

            RecentOrders.Add(new OrderDisplayModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate.ToString(),
                TotalAmount = (double)order.TotalAmount,
                Status = order.Status,
                CustomerName = customer?.Name ?? "Walk-in Customer",
                PaymentMethodName = paymentMethod?.Name ?? "Unknown",
                ServiceTypeName = serviceType?.Name ?? "Unknown"
            });
        }
    }

    private async Task LoadTopProductsAsync()
    {
        TopProducts.Clear();

        var allOrders = await _dao.Orders.GetAll();
        var filteredOrderIds = allOrders.Where(o =>
            DateTime.TryParse(o.OrderDate.ToString(), out var orderDate) &&
            orderDate >= StartDate.DateTime &&
            orderDate <= EndDate.DateTime &&
            o.Status == "Completed")
            .Select(o => o.Id)
            .ToList();

        var allOrderDetails = await _dao.OrderDetails.GetAll();
        var relevantOrderDetails = allOrderDetails.Where(od => filteredOrderIds.Contains(od.OrderId)).ToList();

        var products = await _dao.Products.GetAll();
        var categories = await _dao.Categories.GetAll();

        // Group by product and calculate totals
        var productSales = relevantOrderDetails
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                QuantitySold = g.Sum(od => od.Quantity),
                TotalSales = g.Sum(od => od.Price * od.Quantity)
            })
            .OrderByDescending(x => x.TotalSales)
            .Take(10)  // Top 10 products
            .ToList();

        foreach (var productSale in productSales)
        {
            var product = products.FirstOrDefault(p => p.Id == productSale.ProductId);
            if (product != null)
            {
                var category = categories.FirstOrDefault(c => c.Id == product.CategoryId);

                TopProducts.Add(new ProductSalesViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CategoryName = category?.Name ?? "Uncategorized",
                    Image = product.Image,
                    QuantitySold = productSale.QuantitySold,
                    TotalSales = (double)productSale.TotalSales
                });
            }
        }
    }

    private async Task LoadIngredientCostsAsync()
    {
        IngredientCosts.Clear();

        var allIngredientTransactions = await _dao.IngredientInventoryTransactions.GetAll();
        var filteredTransactions = allIngredientTransactions.Where(t =>
            t.TransactionType == "IMPORT" &&
            DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp) >= StartDate &&
            DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp) <= EndDate).ToList();

        var ingredients = await _dao.Ingredients.GetAll();

        // Group by ingredient and calculate totals
        var ingredientCosts = filteredTransactions
            .GroupBy(t => t.IngredientId)
            .Select(g => new
            {
                IngredientId = g.Key,
                TransactionCount = g.Count(),
                TotalQuantity = g.Sum(t => t.Quantity),
                Unit = g.First().Unit,
                TotalCost = g.Sum(t => t.Quantity * t.UnitPrice)
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        foreach (var cost in ingredientCosts)
        {
            var ingredient = ingredients.FirstOrDefault(i => i.Id == cost.IngredientId);
            if (ingredient != null)
            {
                IngredientCosts.Add(new IngredientCostViewModel
                {
                    IngredientId = ingredient.Id,
                    IngredientName = ingredient.Name,
                    TransactionCount = cost.TransactionCount,
                    TotalQuantity = cost.TotalQuantity,
                    Unit = cost.Unit,
                    TotalCost = cost.TotalCost
                });
            }
        }
    }

    private async Task LoadDistributionAsync()
    {
        ServiceTypeDistribution.Clear();
        PaymentMethodDistribution.Clear();

        var allOrders = await _dao.Orders.GetAll();
        var filteredOrders = allOrders.Where(o =>
            DateTime.TryParse(o.OrderDate.ToString(), out var orderDate) &&
            orderDate >= StartDate.DateTime &&
            orderDate <= EndDate.DateTime &&
            o.Status == "Completed").ToList();

        var serviceTypes = await _dao.ServiceTypes.GetAll();
        var paymentMethods = await _dao.PaymentMethods.GetAll();

        // Total sales for percentage calculations
        double totalSales = (double)filteredOrders.Sum(o => o.TotalAmount);

        // Service Type Distribution
        var serviceTypeStats = filteredOrders
            .GroupBy(o => o.ServiceTypeId)
            .Select(g => new
            {
                ServiceTypeId = g.Key,
                OrderCount = g.Count(),
                TotalSales = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(x => x.TotalSales)
            .ToList();

        foreach (var stat in serviceTypeStats)
        {
            var serviceType = serviceTypes.FirstOrDefault(st => st.Id == stat.ServiceTypeId);
            if (serviceType != null)
            {
                ServiceTypeDistribution.Add(new ServiceTypeViewModel
                {
                    ServiceTypeId = serviceType.Id,
                    ServiceTypeName = serviceType.Name,
                    OrderCount = stat.OrderCount,
                    TotalSales = (double)stat.TotalSales,
                    Percentage = totalSales > 0 ? (double)stat.TotalSales / totalSales : 0
                });
            }
        }

        // Payment Method Distribution
        var paymentMethodStats = filteredOrders
            .GroupBy(o => o.PaymentMethodId)
            .Select(g => new
            {
                PaymentMethodId = g.Key,
                OrderCount = g.Count(),
                TotalSales = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(x => x.TotalSales)
            .ToList();

        foreach (var stat in paymentMethodStats)
        {
            var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.Id == stat.PaymentMethodId);
            if (paymentMethod != null)
            {
                PaymentMethodDistribution.Add(new PaymentMethodViewModel
                {
                    PaymentMethodId = paymentMethod.Id,
                    PaymentMethodName = paymentMethod.Name,
                    OrderCount = stat.OrderCount,
                    TotalSales = (double)stat.TotalSales,
                    Percentage = totalSales > 0 ? (double)stat.TotalSales / totalSales : 0
                });
            }
        }
    }
}

// View models for data binding
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