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

public partial class AddCustomerViewModel : ObservableRecipient
{
    private readonly INavigationService _navigation_service;
    private readonly IDao _dao;
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

    public AddCustomerViewModel(INavigationService navigationService, IDao dao)
    {
        _navigation_service = navigationService;
        _dao = dao;
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public void OnNavigatedTo(object parameter)
    {
        LogMessage("DEBUG", "AddCustomerViewModel.OnNavigatedTo: Initializing AddCustomerPage...");
    }

    public void OnNavigatedFrom()
    {
        LogMessage("DEBUG", "AddCustomerViewModel.OnNavigatedFrom: Leaving AddCustomerPage...");
    }

    [RelayCommand]
    private async Task AddCustomer()
    {
        try
        {
            // Reset error messages
            CustomerNameError = null;
            PhoneError = null;
            PhoneDuplicateError = null;
            LogMessage("DEBUG", "AddCustomerViewModel.AddCustomer: Reset error messages");

            bool hasError = false;

            // Validate CustomerName
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                LogMessage("DEBUG", "AddCustomerViewModel.AddCustomer: CustomerName is empty");
                CustomerNameError = "Customer Name is required.";
                LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Set CustomerNameError = {CustomerNameError}");
                hasError = true;
            }

            // Validate Phone
            if (string.IsNullOrWhiteSpace(Phone))
            {
                LogMessage("DEBUG", "AddCustomerViewModel.AddCustomer: Phone is empty");
                PhoneError = "Phone number is required.";
                LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Set PhoneError = {PhoneError}");
                hasError = true;
            }
            else if (!Phone.All(char.IsDigit))
            {
                LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Invalid phone format - Phone: {Phone}");
                PhoneError = "Phone number must contain only digits.";
                LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Set PhoneError = {PhoneError}");
                hasError = true;
            }

            // Check for duplicate phone number
            if (!hasError)
            {
                var customers = await _dao.Customers.GetAll();
                var existingCustomer = customers.FirstOrDefault(c => c.Phone == Phone);
                if (existingCustomer != null)
                {
                    LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Phone {Phone} already used by customer {existingCustomer.Name} (ID: {existingCustomer.Id})");
                    PhoneDuplicateError = "This phone number is already used by another customer.";
                    LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Set PhoneDuplicateError = {PhoneDuplicateError}");
                    hasError = true;
                }
            }

            if (hasError)
            {
                LogMessage("DEBUG", "AddCustomerViewModel.AddCustomer: Validation failed, returning");
                return;
            }

            // Create new customer
            var newCustomer = new Customer
            {
                Name = CustomerName,
                Phone = Phone
            };

            LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Adding new customer - Name: {newCustomer.Name}, Phone: {newCustomer.Phone}");
            var addedCustomer = await _dao.Customers.Add(newCustomer);
            await _dao.SaveChangesAsync();
            LogMessage("DEBUG", $"AddCustomerViewModel.AddCustomer: Customer added successfully with Id = {addedCustomer.Id}");

            // Show success dialog
            await ShowSuccessDialog("Success", $"Customer {newCustomer.Name} added successfully.");

            // Navigate to CustomersPage
            _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
            LogMessage("DEBUG", "AddCustomerViewModel.AddCustomer: Navigated to CustomersPage");
        }
        catch (Exception ex)
        {
            LogMessage("ERROR", $"AddCustomerViewModel.AddCustomer: {ex.Message}");
            await ShowErrorDialog("Error", $"Failed to add customer: {ex.Message}");
        }
    }

    private async Task ShowErrorDialog(string title, string content)
    {
        if (_isDialogShowing)
        {
            LogMessage("DEBUG", "AddCustomerViewModel.ShowErrorDialog: Another dialog is already showing. Skipping...");
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
            LogMessage("ERROR", $"AddCustomerViewModel.ShowErrorDialog: Failed to show dialog. Exception: {ex.Message}");
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
            LogMessage("DEBUG", "AddCustomerViewModel.ShowSuccessDialog: Another dialog is already showing. Skipping...");
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
            LogMessage("ERROR", $"AddCustomerViewModel.ShowSuccessDialog: Failed to show dialog. Exception: {ex.Message}");
        }
        finally
        {
            _isDialogShowing = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        LogMessage("DEBUG", "AddCustomerViewModel.Cancel: Navigating to CustomersPage");
        _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
    }

    partial void OnCustomerNameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (CustomerNameError != null)
            {
                LogMessage("DEBUG", $"AddCustomerViewModel.OnCustomerNameChanged: Clearing CustomerNameError = {CustomerNameError}");
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
                LogMessage("DEBUG", $"AddCustomerViewModel.OnPhoneChanged: Clearing PhoneError = {PhoneError}");
                PhoneError = null;
            }
            if (PhoneDuplicateError != null)
            {
                LogMessage("DEBUG", $"AddCustomerViewModel.OnPhoneChanged: Clearing PhoneDuplicateError = {PhoneDuplicateError}");
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
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddCustomerViewModel.LogMessage: Failed to show toast notification. Exception: {ex.Message}");
        }
    }
}