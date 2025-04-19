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

public partial class AddCustomerViewModel : ObservableRecipient
{
    private readonly INavigationService _navigation_service;
    private readonly IDao _dao;
    private XamlRoot _xamlRoot;

    [ObservableProperty]
    private string customerName;

    [ObservableProperty]
    private string phone;

    [ObservableProperty]
    private bool isMembership;

    [ObservableProperty]
    private string pointsText;

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
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddCustomerViewModel.OnNavigatedTo: Initializing AddCustomerPage...");
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddCustomerViewModel.OnNavigatedFrom: Leaving AddCustomerPage...");
    }

    [RelayCommand]
    private async Task AddCustomer()
    {
        try
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(Phone))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddCustomerViewModel.AddCustomer: Missing required fields - CustomerName: {CustomerName}, Phone: {Phone}");
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

            // Chuyển đổi PointsText thành int
            if (!int.TryParse(PointsText, out int points) || points < 0)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddCustomerViewModel.AddCustomer: Invalid Points - PointsText: {PointsText}");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Points must be a valid non-negative number.",
                    CloseButtonText = "OK",
                    XamlRoot = _xamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // Kiểm tra số điện thoại trùng lặp
            var customers = await _dao.Customers.GetAll();
            var existingCustomer = customers.FirstOrDefault(c => c.Phone == Phone);
            if (existingCustomer != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddCustomerViewModel.AddCustomer: Phone {Phone} already used by customer {existingCustomer.Name} (ID: {existingCustomer.Id})");
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

            // Tự động đánh giá IsMembership dựa trên Points
            bool calculatedMembership = points >= 100;
            IsMembership = calculatedMembership;

            // Tạo khách hàng mới
            var newCustomer = new Customer
            {
                Name = CustomerName,
                Phone = Phone,
                IsMembership = IsMembership,
                Points = points
            };

            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddCustomerViewModel.AddCustomer: Adding new customer - Name: {newCustomer.Name}, Phone: {newCustomer.Phone}, IsMembership: {newCustomer.IsMembership}");
            var addedCustomer = await _dao.Customers.Add(newCustomer);
            await _dao.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddCustomerViewModel.AddCustomer: Customer added successfully with Id = {addedCustomer.Id}");

            // Chuyển hướng về CustomersPage
            _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddCustomerViewModel.AddCustomer: Navigated to CustomersPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddCustomerViewModel.AddCustomer: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to add customer: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddCustomerViewModel.Cancel: Navigating to CustomersPage");
        _navigation_service.NavigateTo(typeof(CustomersViewModel).FullName);
    }
}