using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using System.Linq;
using Microsoft.UI.Xaml.Media;
using System.Threading;

namespace CoffeePOS.ViewModels;

public partial class EditCustomerViewModel : ObservableRecipient
{
    private readonly INavigationService _navigation_service;
    private readonly IDao _dao;
    private int _customerId;
    private XamlRoot _xamlRoot;
    private bool _isDialogShowing;

    [ObservableProperty]
    private string customerName;

    [ObservableProperty]
    private string phone;

    [ObservableProperty]
    private string customerNameError;

    [ObservableProperty]
    private string phoneError;

    [ObservableProperty]
    private string phoneDuplicateError;

    public EditCustomerViewModel(INavigationService navigationService, IDao dao)
    {
        _navigation_service = navigationService;
        _dao = dao;
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is int customerId)
        {
            _customerId = customerId;
            LogMessage("DEBUG", $"EditCustomerViewModel.OnNavigatedTo: Loading customer with ID = {customerId}");
            await LoadCustomer(customerId);
        }
    }

    private async Task LoadCustomer(int customerId)
    {
        try
        {
            var customer = await _dao.Customers.GetById(customerId);
            if (customer != null)
            {
                CustomerName = customer.Name;
                Phone = customer.Phone;
                LogMessage("DEBUG", $"EditCustomerViewModel.LoadCustomer: Loaded customer - Name: {customer.Name}, Phone: {customer.Phone}");
            }
            else
            {
                LogMessage("ERROR", $"EditCustomerViewModel.LoadCustomer: Customer with ID {customerId} not found");
                await ShowErrorDialog("Error", $"Customer with ID {customerId} not found.");
                _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
            }
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"EditCustomerViewModel.LoadCustomer: {ex.Message}");
            await ShowErrorDialog("Error", $"Failed to load customer: {ex.Message}");
            _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
        }
    }

    public void OnNavigatedFrom()
    {
        LogMessage("DEBUG", "EditCustomerViewModel.OnNavigatedFrom: Leaving EditCustomerPage...");
    }

    [RelayCommand]
    private async Task SaveCustomer()
    {
        try
        {
            // Reset error messages
            CustomerNameError = null;
            PhoneError = null;
            PhoneDuplicateError = null;
            LogMessage("DEBUG", "EditCustomerViewModel.SaveCustomer: Reset error messages");

            bool hasError = false;

            // Validate CustomerName
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                LogMessage("DEBUG", "EditCustomerViewModel.SaveCustomer: CustomerName is empty");
                CustomerNameError = "Customer Name is required.";
                LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Set CustomerNameError = {CustomerNameError}");
                hasError = true;
            }

            // Validate Phone
            if (string.IsNullOrWhiteSpace(Phone))
            {
                LogMessage("DEBUG", "EditCustomerViewModel.SaveCustomer: Phone is empty");
                PhoneError = "Phone number is required.";
                LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Set PhoneError = {PhoneError}");
                hasError = true;
            }
            else if (!Phone.All(char.IsDigit))
            {
                LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Invalid phone format - Phone: {Phone}");
                PhoneError = "Phone number must contain only digits.";
                LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Set PhoneError = {PhoneError}");
                hasError = true;
            }

            // Check for duplicate phone number (excluding current customer)
            if (!hasError)
            {
                var customers = await _dao.Customers.GetAll();
                var existingCustomer = customers.FirstOrDefault(c => c.Phone == Phone && c.Id != _customerId);
                if (existingCustomer != null)
                {
                    LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Phone {Phone} already used by customer {existingCustomer.Name} (ID: {existingCustomer.Id})");
                    PhoneDuplicateError = "This phone number is already used by another customer.";
                    LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Set PhoneDuplicateError = {PhoneDuplicateError}");
                    hasError = true;
                }
            }

            if (hasError)
            {
                LogMessage("DEBUG", "EditCustomerViewModel.SaveCustomer: Validation failed, returning");
                return;
            }

            // Load existing customer to preserve Points and IsMembership
            var customer = await _dao.Customers.GetById(_customerId);
            if (customer == null)
            {
                LogMessage("ERROR", $"EditCustomerViewModel.SaveCustomer: Customer with ID {_customerId} not found");
                await ShowErrorDialog("Error", $"Customer with ID {_customerId} not found.");
                return;
            }

            // Update Name and Phone
            customer.Name = CustomerName;
            customer.Phone = Phone;

            LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Updating customer - ID: {_customerId}, Name: {customer.Name}, Phone: {customer.Phone}");
            await _dao.Customers.Update(customer);
            await _dao.SaveChangesAsync();
            LogMessage("DEBUG", $"EditCustomerViewModel.SaveCustomer: Customer updated successfully - ID: {_customerId}");

            // Show success dialog
            await ShowSuccessDialog("Success", $"Customer {customer.Name} updated successfully.");

            // Navigate to CustomersPage
            _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
            LogMessage("DEBUG", "EditCustomerViewModel.SaveCustomer: Navigated to CustomersPage");
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"EditCustomerViewModel.SaveCustomer: Failed to update customer with ID {_customerId}. Exception: {ex.Message}");
            await ShowErrorDialog("Error", $"Failed to update customer: {ex.Message}");
        }
    }

    private async Task ShowErrorDialog(string title, string content)
    {
        if (_isDialogShowing)
        {
            LogMessage("DEBUG", "EditCustomerViewModel.ShowErrorDialog: Another dialog is already showing. Skipping...");
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
            LogMessage("ERROR", $"EditCustomerViewModel.ShowErrorDialog: Failed to show dialog. Exception: {ex.Message}");
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
            LogMessage("DEBUG", "EditCustomerViewModel.ShowSuccessDialog: Another dialog is already showing. Skipping...");
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
            LogMessage("ERROR", $"EditCustomerViewModel.ShowSuccessDialog: Failed to show dialog. Exception: {ex.Message}");
        }
        finally
        {
            _isDialogShowing = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        LogMessage("DEBUG", "EditCustomerViewModel.Cancel: Navigating to CustomersPage");
        _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
    }

    partial void OnCustomerNameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (CustomerNameError != null)
            {
                LogMessage("DEBUG", $"EditCustomerViewModel.OnCustomerNameChanged: Clearing CustomerNameError = {CustomerNameError}");
                CustomerNameError = null;
            }
        }
    }

    partial void OnPhoneChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (PhoneError != null)
            {
                LogMessage("DEBUG", $"EditCustomerViewModel.OnPhoneChanged: Clearing PhoneError = {PhoneError}");
                PhoneError = null;
            }
            if (PhoneDuplicateError != null)
            {
                LogMessage("DEBUG", $"EditCustomerViewModel.OnPhoneChanged: Clearing PhoneDuplicateError = {PhoneDuplicateError}");
                PhoneDuplicateError = null;
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
            System.Diagnostics.Debug.WriteLine($"[ERROR] EditCustomerViewModel.LogMessage: Failed to show toast notification. Exception: {ex.Message}");
        }
    }
}