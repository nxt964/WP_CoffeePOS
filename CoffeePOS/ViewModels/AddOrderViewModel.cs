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
using Microsoft.UI.Xaml;

namespace CoffeePOS.ViewModels;

public enum ServiceTypeOption
{
    DineIn,
    TakeAway
}

public partial class AddOrderViewModel : ObservableRecipient
{
    private readonly INavigationService _navigationService;
    private readonly IDao _dao;
    private ObservableCollection<Customer> _allCustomers = new ObservableCollection<Customer>();
    private XamlRoot _xamlRoot; // Thêm để quản lý dialog

    [ObservableProperty]
    private ObservableCollection<Customer> customerSuggestions = new ObservableCollection<Customer>();

    [ObservableProperty]
    private ObservableCollection<ServiceType> serviceTypes = new ObservableCollection<ServiceType>();

    private ServiceType _selectedServiceType;
    [ObservableProperty]
    private bool isDineIn;

    [ObservableProperty]
    private bool isTakeAway;

    [ObservableProperty]
    private DateTimeOffset orderDate = DateTimeOffset.Now;

    [ObservableProperty]
    private Customer selectedCustomer;

    [ObservableProperty]
    private string customerName;

    [ObservableProperty]
    private string customerPhone;

    private ServiceTypeOption _selectedServiceTypeOption;

    public AddOrderViewModel(INavigationService navigationService, IDao dao)
    {
        _navigationService = navigationService;
        _dao = dao;
    }

    public ServiceType SelectedServiceType
    {
        get => _selectedServiceType;
        set
        {
            if (SetProperty(ref _selectedServiceType, value))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.SelectedServiceType: Changed to {value?.Name}");
            }
        }
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    partial void OnIsDineInChanged(bool value)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.OnIsDineInChanged: IsDineIn = {value}");
        if (value)
        {
            IsTakeAway = false;
            _selectedServiceTypeOption = ServiceTypeOption.DineIn;
            UpdateSelectedServiceType();
        }
    }

    partial void OnIsTakeAwayChanged(bool value)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.OnIsTakeAwayChanged: IsTakeAway = {value}");
        if (value)
        {
            IsDineIn = false;
            _selectedServiceTypeOption = ServiceTypeOption.TakeAway;
            UpdateSelectedServiceType();
        }
    }

    private async void UpdateSelectedServiceType()
    {
        string serviceTypeName = _selectedServiceTypeOption == ServiceTypeOption.DineIn ? "Dine-in" : "Take-away";
        SelectedServiceType = ServiceTypes.FirstOrDefault(st => st.Name.Equals(serviceTypeName, StringComparison.OrdinalIgnoreCase));

        if (SelectedServiceType == null)
        {
            System.Diagnostics.Debug.WriteLine($"[WARNING] AddOrderViewModel.UpdateSelectedServiceType: ServiceType '{serviceTypeName}' not found in ServiceTypes. Creating new ServiceType...");
            var newServiceType = new ServiceType
            {
                Name = serviceTypeName
            };
            var addedServiceType = await _dao.ServiceTypes.Add(newServiceType);
            await _dao.SaveChangesAsync();
            ServiceTypes.Add(addedServiceType);
            SelectedServiceType = addedServiceType;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.UpdateSelectedServiceType: Created new ServiceType - Id = {addedServiceType.Id}, Name = {addedServiceType.Name}");
        }
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

        ServiceTypes.Clear();
        var serviceTypesList = await _dao.ServiceTypes.GetAll();
        foreach (var serviceType in serviceTypesList)
        {
            ServiceTypes.Add(serviceType);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.OnNavigatedTo: Added ServiceType - Id = {serviceType.Id}, Name = {serviceType.Name}");
        }

        if (ServiceTypes.Any(st => st.Name.Equals("Dine-in", StringComparison.OrdinalIgnoreCase)))
        {
            IsDineIn = true;
        }
        else if (ServiceTypes.Any(st => st.Name.Equals("Take-away", StringComparison.OrdinalIgnoreCase)))
        {
            IsTakeAway = true;
        }
        else
        {
            IsDineIn = true;
        }

        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.OnNavigatedTo: Loaded {serviceTypesList.Count()} service types");
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
            if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(CustomerPhone) || SelectedServiceType == null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Missing required fields - CustomerName: {CustomerName}, CustomerPhone: {CustomerPhone}, SelectedServiceType: {SelectedServiceType?.Name}");
                await ShowDialogAsync("Error", "Please fill in all required fields (Customer Name, Customer Phone, and Service Type).", "OK");
                return;
            }

            // Kiểm tra khách hàng trùng lặp
            var customers = await _dao.Customers.GetAll();
            var existingCustomer = customers.FirstOrDefault(c => c.Phone == CustomerPhone);

            if (existingCustomer != null)
            {
                // Nếu số điện thoại trùng, kiểm tra tên
                if (existingCustomer.Name.Equals(CustomerName, StringComparison.OrdinalIgnoreCase))
                {
                    // Số điện thoại và tên trùng, sử dụng khách hàng hiện có
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Phone {CustomerPhone} and Name {CustomerName} match existing customer {existingCustomer.Name} (ID: {existingCustomer.Id})");
                    SelectedCustomer = existingCustomer;
                }
                else
                {
                    // Số điện thoại trùng nhưng tên không trùng, báo lỗi
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Phone {CustomerPhone} already used by customer {existingCustomer.Name} (ID: {existingCustomer.Id}) with different name");
                    await ShowDialogAsync("Error", "This phone number is already used by another customer with a different name. Please use a different phone number.", "OK");
                    return;
                }
            }
            else
            {
                // Số điện thoại không trùng, tạo khách hàng mới
                var newCustomer = new Customer
                {
                    Name = CustomerName,
                    Phone = CustomerPhone,
                    IsMembership = false,
                    Points = 0
                };
                var addedCustomer = await _dao.Customers.Add(newCustomer);
                await _dao.SaveChangesAsync();
                _allCustomers.Add(addedCustomer);
                SelectedCustomer = addedCustomer;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Created new customer with Id = {SelectedCustomer.Id}");
            }

            // Tạo đơn hàng mới
            var newOrder = new Order
            {
                OrderDate = OrderDate.DateTime,
                CustomerId = SelectedCustomer.Id,
                ServiceTypeId = SelectedServiceType.Id,
                Status = "Pending",
                TotalAmount = 0
            };

            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Adding new order for CustomerId = {newOrder.CustomerId}, ServiceTypeId = {newOrder.ServiceTypeId}");
            var addedOrder = await _dao.Orders.Add(newOrder);
            await _dao.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderViewModel.AddOrder: Order added successfully with Id = {addedOrder.Id}");

            _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddOrderViewModel.AddOrder: Navigated to OrderPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddOrderViewModel.AddOrder: {ex.Message}");
            await ShowDialogAsync("Error", $"Failed to add order: {ex.Message}", "OK");
        }
    }

    // Phương thức helper để hiển thị dialog
    private async Task ShowDialogAsync(string title, string content, string closeButtonText)
    {
        try
        {
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
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddOrderViewModel.ShowDialogAsync: Failed to show dialog. Exception: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddOrderViewModel.Cancel: Navigating to OrderPage");
        _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
    }
}