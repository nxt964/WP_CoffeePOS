using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace CoffeePOS.ViewModels;

public partial class EditEmployeeViewModel : ObservableRecipient
{
    private readonly INavigationService _navigationService;
    private readonly IDao _dao;
    private int _employeeId;

    [ObservableProperty]
    private string employeeName;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string phone;

    [ObservableProperty]
    private string salaryText;

    public EditEmployeeViewModel(INavigationService navigationService, IDao dao)
    {
        _navigationService = navigationService;
        _dao = dao;
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is int employeeId)
        {
            _employeeId = employeeId;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditEmployeeViewModel.OnNavigatedTo: Loading employee with ID = {employeeId}");
            await LoadEmployee(employeeId);
        }
    }

    private async Task LoadEmployee(int employeeId)
    {
        try
        {
            var employee = await _dao.Employees.GetById(employeeId);
            if (employee != null)
            {
                EmployeeName = employee.Name;
                Email = employee.Email;
                Phone = employee.Phone;
                SalaryText = employee.Salary.ToString();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditEmployeeViewModel.LoadEmployee: Loaded employee - Name: {employee.Name}, Phone: {employee.Phone}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] EditEmployeeViewModel.LoadEmployee: Employee with ID {employeeId} not found");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Employee with ID {employeeId} not found.",
                    CloseButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                await dialog.ShowAsync();
                _navigationService.NavigateTo(typeof(EmployeesViewModel).FullName);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] EditEmployeeViewModel.LoadEmployee: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to load employee: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            await dialog.ShowAsync();
            _navigationService.NavigateTo(typeof(EmployeesViewModel).FullName);
        }
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] EditEmployeeViewModel.OnNavigatedFrom: Leaving EditEmployeePage...");
    }

    [RelayCommand]
    private async Task SaveEmployee()
    {
        try
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(EmployeeName) || string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(SalaryText))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditEmployeeViewModel.SaveEmployee: Missing required fields - EmployeeName: {EmployeeName}, Phone: {Phone}, SalaryText: {SalaryText}");
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
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditEmployeeViewModel.SaveEmployee: Invalid Salary - SalaryText: {SalaryText}");
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

            // Kiểm tra số điện thoại trùng lặp (trừ chính nhân viên đang chỉnh sửa)
            var employees = await _dao.Employees.GetAll();
            var existingEmployee = employees.FirstOrDefault(e => e.Phone == Phone && e.Id != _employeeId);
            if (existingEmployee != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditEmployeeViewModel.SaveEmployee: Phone {Phone} already used by employee {existingEmployee.Name}");
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

            // Cập nhật thông tin nhân viên
            var employee = new Employee
            {
                Id = _employeeId,
                Name = EmployeeName,
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email,
                Phone = Phone,
                Salary = salary
            };

            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditEmployeeViewModel.SaveEmployee: Updating employee - ID: {_employeeId}, Name: {employee.Name}, Phone: {employee.Phone}");
            await _dao.Employees.Update(employee);
            await _dao.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditEmployeeViewModel.SaveEmployee: Employee updated successfully - ID: {_employeeId}");

            // Chuyển hướng về EmployeesPage
            _navigationService.NavigateTo(typeof(EmployeesViewModel).FullName);
            System.Diagnostics.Debug.WriteLine("[DEBUG] EditEmployeeViewModel.SaveEmployee: Navigated to EmployeesPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] EditEmployeeViewModel.SaveEmployee: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to update employee: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] EditEmployeeViewModel.Cancel: Navigating to EmployeesPage");
        _navigationService.NavigateTo(typeof(EmployeesViewModel).FullName);
    }
}