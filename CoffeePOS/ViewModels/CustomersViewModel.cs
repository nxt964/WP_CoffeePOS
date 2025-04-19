using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace CoffeePOS.ViewModels;

public partial class CustomersViewModel : ObservableObject, INavigationAware
{
    private readonly IDao _dao;
    private ObservableCollection<CustomerDisplay> _allCustomers = new();
    private XamlRoot _xamlRoot;
    private bool _isDialogShowing; // Theo dõi trạng thái dialog

    [ObservableProperty]
    private ObservableCollection<CustomerDisplay> source = new();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private int pageSize = 5;

    [ObservableProperty]
    private string pageInfo = "1 / 1";

    private int currentPage = 1;
    private int totalPages = 1;

    public CustomersViewModel(IDao dao)
    {
        _dao = dao;
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadCustomers();
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] CustomersViewModel.OnNavigatedFrom: Leaving CustomersPage...");
    }

    private async Task LoadCustomers()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CustomersViewModel.LoadCustomers: Loading customers...");
            _allCustomers.Clear();
            Source.Clear();

            var customers = await _dao.Customers.GetAll();

            foreach (var customer in customers)
            {
                var customerDisplay = new CustomerDisplay
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Phone = customer.Phone,
                    IsMembership = customer.IsMembership,
                    Points = customer.Points
                };
                _allCustomers.Add(customerDisplay);
            }

            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.LoadCustomers: Loaded {_allCustomers.Count} customers");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] CustomersViewModel.LoadCustomers: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Delete(int customerId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.Delete: Attempting to delete CustomerId = {customerId}");

            // Kiểm tra xem khách hàng có đơn hàng không
            var orders = await _dao.Orders.GetAll();
            var hasOrders = orders.Any(o => o.CustomerId == customerId);
            if (hasOrders)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.Delete: CustomerId {customerId} has existing orders. Deletion aborted.");
                await ShowDialogAsync("Cannot Delete Customer", "This customer has existing orders and cannot be deleted.", "OK");
                return;
            }

            // Xóa khách hàng khỏi cơ sở dữ liệu
            await _dao.Customers.Delete(customerId);
            await _dao.SaveChangesAsync();

            // Xóa khách hàng khỏi danh sách hiển thị
            var customer = _allCustomers.FirstOrDefault(c => c.Id == customerId);
            if (customer != null)
            {
                _allCustomers.Remove(customer);
                UpdatePagination();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.Delete: Successfully removed CustomerId {customerId} from display list");
            }

            // Hiển thị thông báo thành công
            await ShowDialogAsync("Success", $"Customer with ID {customerId} has been deleted successfully.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] CustomersViewModel.Delete: Failed to delete CustomerId {customerId}. Exception: {ex.Message}");
            await ShowDialogAsync("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }

    // Phương thức helper để hiển thị dialog và quản lý trạng thái
    private async Task ShowDialogAsync(string title, string content, string closeButtonText)
    {
        // Đảm bảo không hiển thị dialog mới nếu đang có dialog khác
        if (_isDialogShowing)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CustomersViewModel.ShowDialogAsync: Another dialog is already showing. Waiting...");
            return;
        }

        try
        {
            _isDialogShowing = true;
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
            System.Diagnostics.Debug.WriteLine($"[ERROR] CustomersViewModel.ShowDialogAsync: Failed to show dialog. Exception: {ex.Message}");
        }
        finally
        {
            _isDialogShowing = false;
        }
    }

    [RelayCommand]
    private void Search()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.Search: SearchQuery = {SearchQuery}");
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.PreviousPage: CurrentPage = {currentPage}");
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.NextPage: CurrentPage = {currentPage}");
        }
    }

    private void UpdatePagination()
    {
        try
        {
            var filteredCustomers = _allCustomers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filteredCustomers = filteredCustomers.Where(c =>
                    (c.Name != null && c.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Phone != null && c.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));
            }

            var filteredList = filteredCustomers.ToList();
            totalPages = PageSize > 0 ? (int)Math.Ceiling((double)filteredList.Count / PageSize) : 1;
            totalPages = Math.Max(1, totalPages);
            currentPage = Math.Min(currentPage, totalPages);

            Source.Clear();
            var customersToDisplay = filteredList
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            foreach (var customer in customersToDisplay)
            {
                Source.Add(customer);
            }

            PageInfo = $"{currentPage} / {totalPages}";
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.UpdatePagination: PageInfo = {PageInfo}, Displayed Customers = {Source.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] CustomersViewModel.UpdatePagination: {ex.Message}");
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        Search();
    }

    partial void OnPageSizeChanged(int value)
    {
        if (value < 1)
        {
            PageSize = 1;
            return;
        }
        if (value > 50)
        {
            PageSize = 50;
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersViewModel.OnPageSizeChanged: PageSize changed to {value}");
        currentPage = 1;
        UpdatePagination();
    }
}