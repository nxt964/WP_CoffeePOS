using System.ComponentModel;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePOS.ViewModels;

public partial class DashboardViewModel : ObservableRecipient
{
    private readonly IDao _dao;
    public DashboardViewModel(IDao dao)
    {
        _dao = dao;
        LoadData();
    }

    private string _inStock = "";
    public string InStock
    {
        get => _inStock;
        set
        {
            SetProperty(ref _inStock, value);
            OnPropertyChanged(nameof(InStock));
        }
    }

    private string _todayOrders = "";
    public string TodayOrders
    {
        get => _todayOrders;
        set
        {
            SetProperty(ref _todayOrders, value);
            OnPropertyChanged(nameof(TodayOrders));
        }
    }

    private double _todayRevenue = 0.0;
    public double TodayRevenue
    {
        get => _todayRevenue;
        set
        {
            SetProperty(ref _todayRevenue, value);
            OnPropertyChanged(nameof(TodayRevenue));
        }
    }

    private BindingList<Product> _bestSellerInLast7Days = new();
    public BindingList<Product> BestSellerInLast7Days
    {
    
        get => _bestSellerInLast7Days;
        set
        {
            SetProperty(ref _bestSellerInLast7Days, value);
            OnPropertyChanged(nameof(BestSellerInLast7Days));
        }
    }

    private BindingList<Product> _outOfStockProducts = new();
    public BindingList<Product> OutOfStockProducts
    {
    
        get => _outOfStockProducts;
        set
        {
            SetProperty(ref _outOfStockProducts, value);
            OnPropertyChanged(nameof(OutOfStockProducts));
        }
    }

    private BindingList<Order> _latestOrders = new();
    public BindingList<Order> LatestOrders
    {
    
        get => _latestOrders;
        set
        {
            SetProperty(ref _latestOrders, value);
            OnPropertyChanged(nameof(LatestOrders));
        }
    }

    private async void LoadData()
    {
        var products = await _dao.Products.GetAll();
        var inStockCount = products.Count(p => p.IsStocked);
        InStock = $"{inStockCount}/{products.Count()} products";

        var orders = await _dao.Orders.GetAll();
        var todayOrdersCount = orders.Count(o => o.OrderDate.Date == DateTime.Today);
        TodayOrders = $"{todayOrdersCount} orders";

        var todayRevenue = orders.Where(o => o.OrderDate.Date == DateTime.Today)
            .Sum(o => o.TotalAmount);

        TodayRevenue = (double)todayRevenue;

        OutOfStockProducts = new BindingList<Product>(products.Where(p => !p.IsStocked).ToList());


        var ordersInLast7Days = orders.Where(o => o.OrderDate.Date >= DateTime.Today.AddDays(-7))
                                      .Select(o => o.Id)
                                      .ToHashSet();
        var orderDetails = await _dao.OrderDetails.GetAll();
        var bestSeller = orderDetails.Where(od => ordersInLast7Days.Contains(od.OrderId))
                                      .GroupBy(od => od.ProductId)
                                      .Select(g => new
                                      {
                                          ProductId = g.Key,
                                          TotalQuantity = g.Sum(od => od.Quantity)
                                      })
                                      .OrderByDescending(g => g.TotalQuantity)
                                      .Take(5)
                                      .ToList();

        BestSellerInLast7Days = new BindingList<Product>();
        foreach (var item in bestSeller)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null)
            {
                BestSellerInLast7Days.Add(product);
            }
        }


        var latestOrders = orders.OrderByDescending(o => o.OrderDate)
                                  .Take(10)
                                  .ToList();
        LatestOrders = new BindingList<Order>(latestOrders);
    }
}
