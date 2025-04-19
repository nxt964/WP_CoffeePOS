using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace CoffeePOS.ViewModels;

public partial class EditCustomerViewModel : ObservableRecipient
{
    private readonly INavigationService _navigation_service;
    private readonly IDao _dao;
    private int _customerId;
    private XamlRoot _xamlRoot;

    [ObservableProperty]
    private string customerName;

    [ObservableProperty]
    private string phone;

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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditCustomerViewModel.OnNavigatedTo: Loading customer with ID = {customerId}");
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
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditCustomerViewModel.LoadCustomer: Loaded customer - Name: {customer.Name}, Phone: {customer.Phone}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] EditCustomerViewModel.LoadCustomer: Customer with ID {customerId} not found");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Customer with ID {customerId} not found.",
                    CloseButtonText = "OK",
                    XamlRoot = _xamlRoot
                };
                await dialog.ShowAsync();
                _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] EditCustomerViewModel.LoadCustomer: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to load customer: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };
            await dialog.ShowAsync();
            _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
        }
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] EditCustomerViewModel.OnNavigatedFrom: Leaving EditCustomerPage...");
    }

    [RelayCommand]
    private async Task SaveCustomer()
    {
        try
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(Phone))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditCustomerViewModel.SaveCustomer: Missing required fields - CustomerName: {CustomerName}, Phone: {Phone}");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please fill in all required fields (Customer Name and Phone).",
                    CloseButtonText = "OK",
                    XamlRoot = _xamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // Kiểm tra số điện thoại trùng lặp (trừ chính khách hàng đang chỉnh sửa)
            var customers = await _dao.Customers.GetAll();
            var existingCustomer = customers.FirstOrDefault(c => c.Phone == Phone && c.Id != _customerId);
            if (existingCustomer != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditCustomerViewModel.SaveCustomer: Phone {Phone} already used by customer {existingCustomer.Name} (ID: {existingCustomer.Id})");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "This phone number is already used by another customer. Please enter a different phone number.",
                    CloseButtonText = "OK",
                    XamlRoot = _xamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // Lấy khách hàng hiện tại từ cơ sở dữ liệu để giữ nguyên Points và IsMembership
            var customer = await _dao.Customers.GetById(_customerId);
            if (customer == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] EditCustomerViewModel.SaveCustomer: Customer with ID {_customerId} not found");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Customer with ID {_customerId} not found.",
                    CloseButtonText = "OK",
                    XamlRoot = _xamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // Chỉ cập nhật Name và Phone
            customer.Name = CustomerName;
            customer.Phone = Phone;

            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditCustomerViewModel.SaveCustomer: Updating customer - ID: {_customerId}, Name: {customer.Name}, Phone: {customer.Phone}");
            await _dao.Customers.Update(customer);
            await _dao.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditCustomerViewModel.SaveCustomer: Customer updated successfully - ID: {_customerId}");

            // Chuyển hướng về CustomersPage
            _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
            System.Diagnostics.Debug.WriteLine("[DEBUG] EditCustomerViewModel.SaveCustomer: Navigated to CustomersPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] EditCustomerViewModel.SaveCustomer: Failed to update customer with ID {_customerId}. Exception: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to update customer: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] EditCustomerViewModel.Cancel: Navigating to CustomersPage");
        _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
    }
}