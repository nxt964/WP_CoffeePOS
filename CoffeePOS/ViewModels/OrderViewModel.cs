using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CoffeePOS.ViewModels;

public partial class OrderViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;
    private ObservableCollection<OrderDisplay> _allOrdersDisplay = new ObservableCollection<OrderDisplay>(); // Lưu trữ toàn bộ dữ liệu hiển thị

    [ObservableProperty]
    private ObservableCollection<OrderDisplay> source = new ObservableCollection<OrderDisplay>();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private DateTimeOffset fromDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset toDate = DateTimeOffset.Now;

    [ObservableProperty]
    private int pageSize = 10;

    [ObservableProperty]
    private string pageInfo = "1 / 1";

    private int currentPage = 1;
    private int totalPages = 1;

    public OrderViewModel(IDao dao)
    {
        _dao = dao;
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel Constructor: _dao is null: {_dao == null}");
    }

    public async void OnNavigatedTo(object parameter)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.OnNavigatedTo: Loading orders...");
        _allOrdersDisplay.Clear();
        Source.Clear();

        var orders = await _dao.Orders.GetAll();
        var customers = await _dao.Customers.GetAll();

        foreach (var order in orders)
        {
            // Kiểm tra OrderDetails để điều chỉnh TotalAmount
            var orderDetails = (await _dao.OrderDetails.GetAll()).Where(od => od.OrderId == order.Id).ToList();
            if (!orderDetails.Any())
            {
                order.TotalAmount = 0;
            }

            // Join với Customer để lấy Customer Name
            var customer = order.CustomerId.HasValue
                ? customers.FirstOrDefault(c => c.Id == order.CustomerId.Value)
                : null;
            var customerName = customer?.Name ?? "No Customer";

            // Tạo OrderDisplay
            var orderDisplay = new OrderDisplay
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                CustomerName = customerName,
                TotalAmount = order.TotalAmount
            };

            _allOrdersDisplay.Add(orderDisplay);
            Source.Add(orderDisplay);
        }
        UpdatePagination();
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.OnNavigatedFrom: Leaving OrderPage...");
    }

    [RelayCommand]
    private void Add()
    {
        // Logic điều hướng đã được chuyển sang OrderPage.xaml.cs
    }

    [RelayCommand]
    private async Task Delete(string orderId)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Delete: Deleting OrderId = {orderId}");
        if (int.TryParse(orderId, out int id))
        {
            await _dao.Orders.Delete(id);
            await _dao.SaveChangesAsync();

            // Xóa khỏi Source và _allOrdersDisplay
            var orderDisplay = Source.FirstOrDefault(o => o.Id == id);
            if (orderDisplay != null)
            {
                Source.Remove(orderDisplay);
                _allOrdersDisplay.Remove(orderDisplay);
            }
            UpdatePagination();
        }
    }

    [RelayCommand]
    private void Search()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Search: SearchQuery = {SearchQuery}");
        Source.Clear();
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            // Nếu không có từ khóa, hiển thị toàn bộ đơn hàng
            foreach (var order in _allOrdersDisplay)
            {
                Source.Add(order);
            }
        }
        else
        {
            // Lọc đơn hàng dựa trên CustomerName
            var filteredOrders = _allOrdersDisplay
                .Where(o => o.CustomerName != null && o.CustomerName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var order in filteredOrders)
            {
                Source.Add(order);
            }
        }
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void Filter()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Filter: FromDate = {FromDate}, ToDate = {ToDate}");
        Source.Clear();
        var filteredOrders = _allOrdersDisplay
            .Where(o => o.OrderDate >= FromDate.Date && o.OrderDate <= ToDate.Date)
            .ToList();
        foreach (var order in filteredOrders)
        {
            Source.Add(order);
        }
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void RemoveFilter()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.RemoveFilter: Resetting filters...");
        FromDate = DateTimeOffset.Now;
        ToDate = DateTimeOffset.Now;
        SearchQuery = null;
        Source.Clear();
        foreach (var order in _allOrdersDisplay)
        {
            Source.Add(order);
        }
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.PreviousPage: CurrentPage = {currentPage}");
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.NextPage: CurrentPage = {currentPage}");
        }
    }

    public void UpdatePagination()
    {
        totalPages = (int)Math.Ceiling((double)Source.Count / PageSize);
        totalPages = Math.Max(1, totalPages);
        currentPage = Math.Min(currentPage, totalPages);
        PageInfo = $"{currentPage} / {totalPages}";
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.UpdatePagination: PageInfo = {PageInfo}");
    }
}