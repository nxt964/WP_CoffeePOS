using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using CoffeePOS.Services;

namespace CoffeePOS.ViewModels;

public partial class AddOrderViewModel : ObservableRecipient
{
    private readonly INavigationService _navigationService;
    private readonly IDao _dao;
    private ObservableCollection<Customer> _allCustomers = new ObservableCollection<Customer>();

    [ObservableProperty]
    private ObservableCollection<Customer> customerSuggestions = new ObservableCollection<Customer>();

    [ObservableProperty]
    private DateTimeOffset orderDate = DateTimeOffset.Now;

    [ObservableProperty]
    private Customer selectedCustomer;

    [ObservableProperty]
    private string customerName;

    [ObservableProperty]
    private string customerPhone;

    public AddOrderViewModel(INavigationService navigationService, IDao dao)
    {
        _navigationService = navigationService;
        _dao = dao;
    }

    public async void OnNavigatedTo(object parameter)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddOrderViewModel.OnNavigatedTo: Loading customers...");
        _allCustomers.Clear();
        CustomerSuggestions.Clear();
        var customersList = await _dao.Customers.GetAll();
        foreach (var customer in customersList)
        {
            _allCustomers.Add(customer);
            CustomerSuggestions.Add(customer);
        }
        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.OnNavigatedTo: Loaded {customersList.Count()} customers");
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddOrderViewModel.OnNavigatedFrom: Leaving page...");
    }

    public void UpdateCustomerSuggestions(string query)
    {
        CustomerSuggestions.Clear();
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var customer in _allCustomers)
            {
                CustomerSuggestions.Add(customer);
            }
        }
        else
        {
            var filteredCustomers = _allCustomers
                .Where(c => c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var customer in filteredCustomers)
            {
                CustomerSuggestions.Add(customer);
            }
        }
        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.UpdateCustomerSuggestions: Found {CustomerSuggestions.Count} suggestions for query '{query}'");
    }

    [RelayCommand]
    private async Task AddOrder()
    {
        try
        {
            // Kiểm tra nếu thiếu trường
            if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(CustomerPhone))
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] AddOrderViewModel.AddOrder: Missing required fields");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please fill in both customer name and phone number.",
                    CloseButtonText = "OK"
                };
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
                await dialog.ShowAsync();
                return;
            }

            // Lấy danh sách khách hàng mới nhất từ _dao và kiểm tra số điện thoại
            var customers = await _dao.Customers.GetAll();
            var existingCustomer = customers.FirstOrDefault(c => c.Phone == CustomerPhone);

            if (existingCustomer != null)
            {
                // Nếu số điện thoại đã tồn tại, báo lỗi
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Phone {CustomerPhone} already used by customer {existingCustomer.Name}");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "This phone number is already used by another customer. Please enter a different phone number.",
                    CloseButtonText = "OK"
                };
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
                await dialog.ShowAsync();
                return;
            }

            // Nếu số điện thoại không tồn tại, tạo khách hàng mới
            var newCustomer = new Customer
            {
                Name = CustomerName,
                Phone = CustomerPhone,
                IsMembership = false, // Giá trị mặc định
                Points = 0 // Giá trị mặc định
            };
            var addedCustomer = await _dao.Customers.Add(newCustomer);
            await _dao.SaveChangesAsync();
            _allCustomers.Add(addedCustomer); // Cập nhật danh sách khách hàng
            SelectedCustomer = addedCustomer;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Created new customer with Id = {SelectedCustomer.Id}");

            // Tạo đối tượng Order mới
            var newOrder = new Order
            {
                OrderDate = OrderDate.DateTime,
                CustomerId = SelectedCustomer.Id
            };

            // Thêm đơn hàng mới vào database
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Adding new order for CustomerId = {newOrder.CustomerId}");
            var addedOrder = await _dao.Orders.Add(newOrder);
            await _dao.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Order added successfully with Id = {addedOrder.Id}");

            // Điều hướng về trang danh sách đơn hàng
            _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddOrderViewModel.AddOrder: Navigated to OrderPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddOrderViewModel.AddOrder: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to add order: {ex.Message}",
                CloseButtonText = "OK"
            };
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            await dialog.ShowAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddOrderViewModel.Cancel: Navigating to OrderPage");
        _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
    }
}