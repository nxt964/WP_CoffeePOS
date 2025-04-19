using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace CoffeePOS.ViewModels;

public partial class AddEmployeeViewModel : ObservableRecipient
{
    private readonly INavigationService _navigationService;
    private readonly IDao _dao;

    [ObservableProperty]
    private string employeeName;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string phone;

    [ObservableProperty]
    private string salaryText; // Sử dụng string để binding với TextBox, sau đó chuyển thành decimal

    public AddEmployeeViewModel(INavigationService navigationService, IDao dao)
    {
        _navigationService = navigationService;
        _dao = dao;
    }

    public void OnNavigatedTo(object parameter)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddEmployeeViewModel.OnNavigatedTo: Initializing AddEmployeePage...");
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddEmployeeViewModel.OnNavigatedFrom: Leaving AddEmployeePage...");
    }

    [RelayCommand]
    private async Task AddEmployee()
    {
        try
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(EmployeeName) || string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(SalaryText))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddEmployeeViewModel.AddEmployee: Missing required fields - EmployeeName: {EmployeeName}, Phone: {Phone}, SalaryText: {SalaryText}");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please fill in all required fields (Employee Name, Phone, and Salary).",
                    CloseButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // Chuyển đổi SalaryText thành decimal
            if (!decimal.TryParse(SalaryText, out decimal salary) || salary < 0)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddEmployeeViewModel.AddEmployee: Invalid Salary - SalaryText: {SalaryText}");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Salary must be a valid non-negative number.",
                    CloseButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // Kiểm tra số điện thoại trùng lặp
            var employees = await _dao.Employees.GetAll();
            var existingEmployee = employees.FirstOrDefault(e => e.Phone == Phone);
            if (existingEmployee != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddEmployeeViewModel.AddEmployee: Phone {Phone} already used by employee {existingEmployee.Name}");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "This phone number is already used by another employee. Please enter a different phone number.",
                    CloseButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // Tạo nhân viên mới
            var newEmployee = new Employee
            {
                Name = EmployeeName,
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email,
                Phone = Phone,
                Salary = salary
            };

            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddEmployeeViewModel.AddEmployee: Adding new employee - Name: {newEmployee.Name}, Phone: {newEmployee.Phone}");
            var addedEmployee = await _dao.Employees.Add(newEmployee);
            await _dao.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddEmployeeViewModel.AddEmployee: Employee added successfully with Id = {addedEmployee.Id}");

            // Chuyển hướng về EmployeesPage
            _navigationService.NavigateTo(typeof(EmployeesViewModel).FullName);
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddEmployeeViewModel.AddEmployee: Navigated to EmployeesPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddEmployeeViewModel.AddEmployee: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to add employee: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddEmployeeViewModel.Cancel: Navigating to EmployeesPage");
        _navigationService.NavigateTo(typeof(EmployeesViewModel).FullName);
    }
}