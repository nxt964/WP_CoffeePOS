using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
    private DateTimeOffset fromDate = DateTimeOffset.Now.AddDays(-30);

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
        await LoadOrders();
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.OnNavigatedFrom: Leaving OrderPage...");
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadOrders();
    }

    private async Task LoadOrders()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.LoadOrders: Loading orders...");
            _allOrdersDisplay.Clear();
            Source.Clear();

            var orders = await _dao.Orders.GetAll();
            var customers = await _dao.Customers.GetAll();
            var serviceTypes = await _dao.ServiceTypes.GetAll();

            if (!orders.Any())
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.LoadOrders: No orders found in database.");
            }

            foreach (var order in orders)
            {
                var customer = order.CustomerId.HasValue
                    ? customers.FirstOrDefault(c => c.Id == order.CustomerId.Value)
                    : null;
                var customerName = customer?.Name ?? "No Customer";

                var serviceType = order.ServiceTypeId.HasValue
                    ? serviceTypes.FirstOrDefault(st => st.Id == order.ServiceTypeId.Value)
                    : null;
                var serviceTypeName = serviceType?.Name ?? "No Service Type";

                var orderDisplay = new OrderDisplay
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    CustomerName = customerName,
                    TotalAmount = order.TotalAmount,
                    ServiceTypeName = serviceTypeName,
                    Status = order.Status
                };

                _allOrdersDisplay.Add(orderDisplay);
            }

            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.LoadOrders: Loaded {_allOrdersDisplay.Count} orders");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OrderViewModel.LoadOrders: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Add()
    {
        // TODO: Implement Add logic
    }

    private bool CanDeleteOrder(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId) || !int.TryParse(orderId, out int id))
        {
            return false;
        }

        var order = _allOrdersDisplay.FirstOrDefault(o => o.Id == id);
        return order != null; // Bỏ điều kiện kiểm tra trạng thái "Complete"
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrder))]
    private async Task Delete(string orderId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Delete: Deleting OrderId = {orderId}");
            if (int.TryParse(orderId, out int id))
            {
                var order = _allOrdersDisplay.FirstOrDefault(o => o.Id == id);
                if (order == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] OrderViewModel.Delete: OrderId {id} not found in _allOrdersDisplay");
                    return;
                }

                // Xóa các chi tiết đơn hàng liên quan trước
                var orderDetails = (await _dao.OrderDetails.GetAll()).Where(od => od.OrderId == id).ToList();
                foreach (var detail in orderDetails)
                {
                    await _dao.OrderDetails.Delete(detail.Id);
                }

                await _dao.Orders.Delete(id);
                await _dao.SaveChangesAsync();

                _allOrdersDisplay.Remove(order);
                UpdatePagination();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Delete: Successfully deleted OrderId {id}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OrderViewModel.Delete: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Search()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Search: SearchQuery = {SearchQuery}");
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
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void RemoveFilter()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.RemoveFilter: Resetting filters...");
        FromDate = DateTimeOffset.Now.AddDays(-30);
        ToDate = DateTimeOffset.Now;
        SearchQuery = null;
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

    private void UpdatePagination()
    {
        try
        {
            var filteredOrders = _allOrdersDisplay.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filteredOrders = filteredOrders.Where(o => o.CustomerName != null && o.CustomerName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            filteredOrders = filteredOrders.Where(o => o.OrderDate >= FromDate.Date && o.OrderDate <= ToDate.Date.AddDays(1).AddTicks(-1));

            var filteredList = filteredOrders.ToList();
            totalPages = PageSize > 0 ? (int)Math.Ceiling((double)filteredList.Count / PageSize) : 1;
            totalPages = Math.Max(1, totalPages);
            currentPage = Math.Min(currentPage, totalPages);

            Source.Clear();
            var ordersToDisplay = filteredList
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OrderViewModel.UpdatePagination: {ex.Message}");
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        Search();
    }

    partial void OnFromDateChanged(DateTimeOffset value)
    {
        Filter();
    }

    partial void OnToDateChanged(DateTimeOffset value)
    {
        Filter();
    }
}