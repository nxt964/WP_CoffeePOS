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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace CoffeePOS.ViewModels;

public partial class OrderViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;
    private ObservableCollection<OrderDisplay> _allOrdersDisplay = new ObservableCollection<OrderDisplay>();
    private XamlRoot _xamlRoot;
    private bool _isDialogShowing;

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
        LogMessage("DEBUG", $"OrderViewModel Constructor: _dao is null: {_dao == null}");
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                LogMessage("DEBUG", $"OrderViewModel.PageSize: Changed to {value}");
                currentPage = 1;
                UpdatePagination();
            }
        }
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadOrders();
    }

    public void OnNavigatedFrom()
    {
        LogMessage("DEBUG", "OrderViewModel.OnNavigatedFrom: Leaving OrderPage...");
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
            LogMessage("DEBUG", "OrderViewModel.LoadOrders: Loading orders...");
            _allOrdersDisplay.Clear();
            Source.Clear();

            var orders = await _dao.Orders.GetAll();
            var customers = await _dao.Customers.GetAll();
            var serviceTypes = await _dao.ServiceTypes.GetAll();

            if (!orders.Any())
            {
                LogMessage("DEBUG", "OrderViewModel.LoadOrders: No orders found in database.");
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
            LogMessage("DEBUG", $"OrderViewModel.LoadOrders: Loaded {_allOrdersDisplay.Count} orders");
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"OrderViewModel.LoadOrders: {ex.Message}");
            await ShowDialogAsync("Error", $"Failed to load orders: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void Add()
    {
        // Handled in OrderPage.xaml.cs
    }

    private bool CanDeleteOrder(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId) || !int.TryParse(orderId, out int id))
        {
            return false;
        }

        var order = _allOrdersDisplay.FirstOrDefault(o => o.Id == id);
        return order != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrder))]
    private async Task Delete(string orderId)
    {
        try
        {
            LogMessage("DEBUG", $"OrderViewModel.Delete: Attempting to delete OrderId = {orderId}");
            if (!int.TryParse(orderId, out int id))
            {
                LogMessage("ERROR", $"OrderViewModel.Delete: Invalid OrderId = {orderId}");
                await ShowDialogAsync("Error", "Invalid order ID.", "OK");
                return;
            }

            var order = _allOrdersDisplay.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                LogMessage("ERROR", $"OrderViewModel.Delete: OrderId {id} not found in _allOrdersDisplay");
                await ShowDialogAsync("Error", "Order not found.", "OK");
                return;
            }

            // Delete related order details
            var orderDetails = (await _dao.OrderDetails.GetAll()).Where(od => od.OrderId == id).ToList();
            foreach (var detail in orderDetails)
            {
                await _dao.OrderDetails.Delete(detail.Id);
            }

            await _dao.Orders.Delete(id);
            await _dao.SaveChangesAsync();

            _allOrdersDisplay.Remove(order);
            UpdatePagination();
            LogMessage("DEBUG", $"OrderViewModel.Delete: Successfully deleted OrderId = {id}");
            await ShowDialogAsync("Success", $"Order ID {id} has been deleted successfully.", "OK");
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"OrderViewModel.Delete: Failed to delete OrderId = {orderId}. Exception: {ex.Message}");
            await ShowDialogAsync("Error", $"Failed to delete order: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void Search()
    {
        LogMessage("DEBUG", $"OrderViewModel.Search: SearchQuery = {SearchQuery}");
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void Filter()
    {
        LogMessage("DEBUG", $"OrderViewModel.Filter: FromDate = {FromDate}, ToDate = {ToDate}");
        if (FromDate > ToDate)
        {
            LogMessage("DEBUG", "OrderViewModel.Filter: FromDate is greater than ToDate, swapping...");
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
        LogMessage("DEBUG", "OrderViewModel.RemoveFilter: Resetting filters...");
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
            LogMessage("DEBUG", $"OrderViewModel.PreviousPage: CurrentPage = {currentPage}");
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            UpdatePagination();
            LogMessage("DEBUG", $"OrderViewModel.NextPage: CurrentPage = {currentPage}");
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
            LogMessage("DEBUG", $"OrderViewModel.UpdatePagination: PageInfo = {PageInfo}, Displayed Orders = {Source.Count}");
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"OrderViewModel.UpdatePagination: {ex.Message}");
            // No dialog to avoid spamming during pagination
        }
    }

    private async Task ShowDialogAsync(string title, string content, string closeButtonText)
    {
        if (_isDialogShowing)
        {
            LogMessage("DEBUG", $"OrderViewModel.ShowDialogAsync: Another dialog is already showing. Skipping...");
            return;
        }

        try
        {
            _isDialogShowing = true;
            LogMessage("DEBUG", $"OrderViewModel.ShowDialogAsync: Showing dialog - Title: {title}, Content: {content}");
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = closeButtonText,
                XamlRoot = _xamlRoot
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"OrderViewModel.ShowDialogAsync: Failed to show dialog. Exception: {ex.Message}");
        }
        finally
        {
            _isDialogShowing = false;
        }
    }

    private async void LogMessage(string type, string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] [{type}] {message}";

            // Write to debug console
            System.Diagnostics.Debug.WriteLine(logMessage);

            if (_xamlRoot == null)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                XamlRoot = _xamlRoot,
                Content = logMessage,
                Style = Application.Current.Resources["ToastNotificationStyle"] as Style,
                Background = type == "ERROR" ? new SolidColorBrush(Microsoft.UI.Colors.Red) : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 233, 236, 239)),
                Foreground = type == "ERROR" ? new SolidColorBrush(Microsoft.UI.Colors.White) : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 52, 58, 64)),
            };

            // Run the dialog in the background and auto-dismiss after 3 seconds
            _ = Task.Run(async () =>
            {
                await dialog.ShowAsync();
                await Task.Delay(3000);
                dialog.Hide();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OrderViewModel.LogMessage: Failed to show toast notification. Exception: {ex.Message}");
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