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
using Microsoft.UI.Xaml.Media;
using System.Threading;

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
    private XamlRoot _xamlRoot;
    private bool _isDialogShowing;

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

    [ObservableProperty]
    private string customerNameError;

    [ObservableProperty]
    private string customerPhoneError;

    [ObservableProperty]
    private string customerPhoneDuplicateError;

    [ObservableProperty]
    private string serviceTypeError;

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
                LogMessage("DEBUG", $"AddOrderViewModel.SelectedServiceType: Changed to {value?.Name}");
            }
        }
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    partial void OnIsDineInChanged(bool value)
    {
        LogMessage("DEBUG", $"AddOrderViewModel.OnIsDineInChanged: IsDineIn = {value}");
        if (value)
        {
            IsTakeAway = false;
            _selectedServiceTypeOption = ServiceTypeOption.DineIn;
            UpdateSelectedServiceType();
            if (ServiceTypeError != null)
            {
                LogMessage("DEBUG", $"AddOrderViewModel.OnIsDineInChanged: Clearing ServiceTypeError = {ServiceTypeError}");
                ServiceTypeError = null;
            }
        }
    }

    partial void OnIsTakeAwayChanged(bool value)
    {
        LogMessage("DEBUG", $"AddOrderViewModel.OnIsTakeAwayChanged: IsTakeAway = {value}");
        if (value)
        {
            IsDineIn = false;
            _selectedServiceTypeOption = ServiceTypeOption.TakeAway;
            UpdateSelectedServiceType();
            if (ServiceTypeError != null)
            {
                LogMessage("DEBUG", $"AddOrderViewModel.OnIsTakeAwayChanged: Clearing ServiceTypeError = {ServiceTypeError}");
                ServiceTypeError = null;
            }
        }
    }

    private async void UpdateSelectedServiceType()
    {
        string serviceTypeName = _selectedServiceTypeOption == ServiceTypeOption.DineIn ? "Dine-in" : "Take-away";
        SelectedServiceType = ServiceTypes.FirstOrDefault(st => st.Name.Equals(serviceTypeName, StringComparison.OrdinalIgnoreCase));

        if (SelectedServiceType == null)
        {
            LogMessage("WARNING", $"AddOrderViewModel.UpdateSelectedServiceType: ServiceType '{serviceTypeName}' not found in ServiceTypes. Creating new ServiceType...");
            var newServiceType = new ServiceType
            {
                Name = serviceTypeName
            };
            var addedServiceType = await _dao.ServiceTypes.Add(newServiceType);
            await _dao.SaveChangesAsync();
            ServiceTypes.Add(addedServiceType);
            SelectedServiceType = addedServiceType;
            LogMessage("DEBUG", $"AddOrderViewModel.UpdateSelectedServiceType: Created new ServiceType - Id = {addedServiceType.Id}, Name = {addedServiceType.Name}");
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        LogMessage("DEBUG", "AddOrderViewModel.OnNavigatedTo: Loading customers...");
        _allCustomers.Clear();
        CustomerSuggestions.Clear();
        var customersList = await _dao.Customers.GetAll();
        foreach (var customer in customersList)
        {
            _allCustomers.Add(customer);
            CustomerSuggestions.Add(customer);
        }
        LogMessage("DEBUG", $"AddOrderViewModel.OnNavigatedTo: Loaded {customersList.Count()} customers");

        ServiceTypes.Clear();
        var serviceTypesList = await _dao.ServiceTypes.GetAll();
        foreach (var serviceType in serviceTypesList)
        {
            ServiceTypes.Add(serviceType);
            LogMessage("DEBUG", $"AddOrderViewModel.OnNavigatedTo: Added ServiceType - Id = {serviceType.Id}, Name = {serviceType.Name}");
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

        LogMessage("DEBUG", $"AddOrderViewModel.OnNavigatedTo: Loaded {serviceTypesList.Count()} service types");
    }

    public void OnNavigatedFrom()
    {
        LogMessage("DEBUG", "AddOrderViewModel.OnNavigatedFrom: Leaving page...");
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
        LogMessage("DEBUG", $"AddOrderViewModel.UpdateCustomerSuggestions: Found {CustomerSuggestions.Count} suggestions for query '{query}'");
    }

    [RelayCommand]
    private async Task AddOrder()
    {
        try
        {
            // Reset error messages
            CustomerNameError = null;
            CustomerPhoneError = null;
            CustomerPhoneDuplicateError = null;
            ServiceTypeError = null;
            LogMessage("DEBUG", "AddOrderViewModel.AddOrder: Reset error messages");

            bool hasError = false;

            // Validate CustomerName
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                LogMessage("DEBUG", "AddOrderViewModel.AddOrder: CustomerName is empty");
                CustomerNameError = "Customer Name is required.";
                LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Set CustomerNameError = {CustomerNameError}");
                hasError = true;
            }

            // Validate CustomerPhone
            if (string.IsNullOrWhiteSpace(CustomerPhone))
            {
                LogMessage("DEBUG", "AddOrderViewModel.AddOrder: CustomerPhone is empty");
                CustomerPhoneError = "Customer Phone is required.";
                LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Set CustomerPhoneError = {CustomerPhoneError}");
                hasError = true;
            }
            else if (!CustomerPhone.All(char.IsDigit))
            {
                LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Invalid phone format - CustomerPhone: {CustomerPhone}");
                CustomerPhoneError = "Customer Phone must contain only digits.";
                LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Set CustomerPhoneError = {CustomerPhoneError}");
                hasError = true;
            }

            // Validate ServiceType
            if (!IsDineIn && !IsTakeAway)
            {
                LogMessage("DEBUG", "AddOrderViewModel.AddOrder: No service type selected");
                ServiceTypeError = "Please select a service type.";
                LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Set ServiceTypeError = {ServiceTypeError}");
                hasError = true;
            }

            // Check for duplicate phone with different name
            if (!hasError)
            {
                var customers = await _dao.Customers.GetAll();
                var existingCustomer = customers.FirstOrDefault(c => c.Phone == CustomerPhone);

                if (existingCustomer != null)
                {
                    if (!existingCustomer.Name.Equals(CustomerName, StringComparison.OrdinalIgnoreCase))
                    {
                        LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Phone {CustomerPhone} already used by customer {existingCustomer.Name} (ID: {existingCustomer.Id}) with different name");
                        CustomerPhoneDuplicateError = "This phone number is already used by another customer with a different name.";
                        LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Set CustomerPhoneDuplicateError = {CustomerPhoneDuplicateError}");
                        hasError = true;
                    }
                    else
                    {
                        LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Phone {CustomerPhone} and Name {CustomerName} match existing customer {existingCustomer.Name} (ID: {existingCustomer.Id})");
                        SelectedCustomer = existingCustomer;
                    }
                }
            }

            if (hasError)
            {
                LogMessage("DEBUG", "AddOrderViewModel.AddOrder: Validation failed, returning");
                return;
            }

            // Create new customer if phone doesn't exist
            if (SelectedCustomer == null)
            {
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
                LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Created new customer with Id = {SelectedCustomer.Id}");
            }

            // Create new order
            var newOrder = new Order
            {
                OrderDate = OrderDate.DateTime,
                CustomerId = SelectedCustomer.Id,
                ServiceTypeId = SelectedServiceType.Id,
                Status = "Pending",
                TotalAmount = 0
            };

            LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Adding new order for CustomerId = {newOrder.CustomerId}, ServiceTypeId = {newOrder.ServiceTypeId}");
            var addedOrder = await _dao.Orders.Add(newOrder);
            await _dao.SaveChangesAsync();
            LogMessage("DEBUG", $"AddOrderViewModel.AddOrder: Order added successfully with Id = {addedOrder.Id}");

            // Show success dialog
            await ShowSuccessDialog("Success", "Order added successfully.");

            _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
            LogMessage("DEBUG", "AddOrderViewModel.AddOrder: Navigated to OrderPage");
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"AddOrderViewModel.AddOrder: {ex.Message}");
            await ShowErrorDialog("Error", $"Failed to add order: {ex.Message}");
        }
    }

    private async Task ShowErrorDialog(string title, string content)
    {
        if (_isDialogShowing)
        {
            LogMessage("DEBUG", "AddOrderViewModel.ShowErrorDialog: Another dialog is already showing. Skipping...");
            return;
        }

        try
        {
            _isDialogShowing = true;
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"AddOrderViewModel.ShowErrorDialog: Failed to show dialog. Exception: {ex.Message}");
        }
        finally
        {
            _isDialogShowing = false;
        }
    }

    private async Task ShowSuccessDialog(string title, string content)
    {
        if (_isDialogShowing)
        {
            LogMessage("DEBUG", "AddOrderViewModel.ShowSuccessDialog: Another dialog is already showing. Skipping...");
            return;
        }

        try
        {
            _isDialogShowing = true;
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"AddOrderViewModel.ShowSuccessDialog: Failed to show dialog. Exception: {ex.Message}");
        }
        finally
        {
            _isDialogShowing = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        LogMessage("DEBUG", "AddOrderViewModel.Cancel: Navigating to OrderPage");
        _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
    }

    partial void OnCustomerNameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (CustomerNameError != null)
            {
                LogMessage("DEBUG", $"AddOrderViewModel.OnCustomerNameChanged: Clearing CustomerNameError = {CustomerNameError}");
                CustomerNameError = null;
            }
        }
    }

    partial void OnCustomerPhoneChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (CustomerPhoneError != null)
            {
                LogMessage("DEBUG", $"AddOrderViewModel.OnCustomerPhoneChanged: Clearing CustomerPhoneError = {CustomerPhoneError}");
                CustomerPhoneError = null;
            }
            if (CustomerPhoneDuplicateError != null)
            {
                LogMessage("DEBUG", $"AddOrderViewModel.OnCustomerPhoneChanged: Clearing CustomerPhoneDuplicateError = {CustomerPhoneDuplicateError}");
                CustomerPhoneDuplicateError = null;
            }
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
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddOrderViewModel.LogMessage: Failed to show toast notification. Exception: {ex.Message}");
        }
    }
}