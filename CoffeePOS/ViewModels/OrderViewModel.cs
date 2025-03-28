using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using CoffeePOS.Models;

namespace CoffeePOS.ViewModels;

public partial class OrderViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;
    private ObservableCollection<OrderDisplay> _allOrdersDisplay = new ObservableCollection<OrderDisplay>();

    [ObservableProperty]
    private ObservableCollection<OrderDisplay> source = new ObservableCollection<OrderDisplay>();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private DateTimeOffset fromDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset toDate = DateTimeOffset.Now;

    private int _pageSize = 10;
    [ObservableProperty]
    private string pageInfo = "1 / 1";

    private int currentPage = 1;
    private int totalPages = 1;

    public OrderViewModel(IDao dao)
    {
        _dao = dao;
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel Constructor: _dao is null: {_dao == null}");
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.PageSize: Changed to {value}");
                currentPage = 1;
                UpdatePagination();
            }
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.OnNavigatedTo: Loading orders...");
        _allOrdersDisplay.Clear();
        Source.Clear();

        var orders = await _dao.Orders.GetAll();
        var customers = await _dao.Customers.GetAll();
        var serviceTypes = await _dao.ServiceTypes.GetAll();

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

            var serviceType = order.ServiceTypeId.HasValue ? serviceTypes.FirstOrDefault(st => st.Id == order.ServiceTypeId.Value) : null;
            var serviceTypeName = serviceType?.Name ?? "No Service Type";


            // Tạo OrderDisplay
            var orderDisplay = new OrderDisplay
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                CustomerName = customerName,
                TotalAmount = order.TotalAmount,
                ServiceTypeName = serviceTypeName // Gán giá trị cho ServiceTypeName
            };

            _allOrdersDisplay.Add(orderDisplay);
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

            var orderDisplay = _allOrdersDisplay.FirstOrDefault(o => o.Id == id);
            if (orderDisplay != null)
            {
                _allOrdersDisplay.Remove(orderDisplay);
            }
            UpdatePagination();
        }
    }

    [RelayCommand]
    private void Search()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Search: SearchQuery = {SearchQuery}");
        var filteredOrders = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allOrdersDisplay.ToList()
            : _allOrdersDisplay
                .Where(o => o.CustomerName != null && o.CustomerName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _allOrdersDisplay.Clear();
        foreach (var order in filteredOrders)
        {
            _allOrdersDisplay.Add(order);
        }
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void Filter()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Filter: FromDate = {FromDate}, ToDate = {ToDate}");
        if (FromDate > ToDate)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.Filter: FromDate is greater than ToDate, swapping...");
            var temp = FromDate;
            FromDate = ToDate;
            ToDate = temp;
        }

        var filteredOrders = _allOrdersDisplay
            .Where(o => o.OrderDate >= FromDate.Date && o.OrderDate <= ToDate.Date)
            .ToList();

        _allOrdersDisplay.Clear();
        foreach (var order in filteredOrders)
        {
            _allOrdersDisplay.Add(order);
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

        _allOrdersDisplay.Clear();
        OnNavigatedTo(null);
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
        totalPages = PageSize > 0 ? (int)Math.Ceiling((double)_allOrdersDisplay.Count / PageSize) : 1;
        totalPages = Math.Max(1, totalPages);
        currentPage = Math.Min(currentPage, totalPages);

        Source.Clear();
        var ordersToDisplay = _allOrdersDisplay
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        foreach (var order in ordersToDisplay)
        {
            Source.Add(order);
        }

        PageInfo = $"{currentPage} / {totalPages}";
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.UpdatePagination: PageInfo = {PageInfo}, Displayed Orders = {Source.Count}");
    }
}