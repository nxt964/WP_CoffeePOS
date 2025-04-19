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

namespace CoffeePOS.ViewModels;

public partial class EmployeesViewModel : ObservableObject, INavigationAware
{
    private readonly IDao _dao;
    private ObservableCollection<EmployeeDisplay> _allEmployees = new();

    [ObservableProperty]
    private ObservableCollection<EmployeeDisplay> source = new();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private int pageSize = 5;

    [ObservableProperty]
    private string pageInfo = "1 / 1";

    private int currentPage = 1;
    private int totalPages = 1;

    public EmployeesViewModel(IDao dao)
    {
        _dao = dao;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadEmployees();
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] EmployeesViewModel.OnNavigatedFrom: Leaving EmployeesPage...");
    }

    private async Task LoadEmployees()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] EmployeesViewModel.LoadEmployees: Loading employees...");
            _allEmployees.Clear();
            Source.Clear();

            var employees = await _dao.Employees.GetAll();

            foreach (var employee in employees)
            {
                var employeeDisplay = new EmployeeDisplay
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Email = employee.Email,
                    Phone = employee.Phone,
                    Salary = employee.Salary != 0 ? employee.Salary : 0m
                };
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.LoadEmployees: Employee ID = {employee.Id}, Salary = {employee.Salary}");
                _allEmployees.Add(employeeDisplay);
            }

            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.LoadEmployees: Loaded {_allEmployees.Count} employees");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] EmployeesViewModel.LoadEmployees: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Add()
    {
        // Không còn cần thiết vì đã xử lý trong EmployeesPage.xaml.cs
    }

    [RelayCommand]
    private void Delete(int employeeId)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.Delete: Removing EmployeeId = {employeeId} from display list");
        var employee = _allEmployees.FirstOrDefault(e => e.Id == employeeId);
        if (employee != null)
        {
            _allEmployees.Remove(employee);
            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.Delete: Successfully removed EmployeeId {employeeId} from display list");
        }
    }

    [RelayCommand]
    private void Search()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.Search: SearchQuery = {SearchQuery}");
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.PreviousPage: CurrentPage = {currentPage}");
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.NextPage: CurrentPage = {currentPage}");
        }
    }

    private void UpdatePagination()
    {
        try
        {
            var filteredEmployees = _allEmployees.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filteredEmployees = filteredEmployees.Where(e =>
                    (e.Name != null && e.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Email != null && e.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));
            }

            var filteredList = filteredEmployees.ToList();
            totalPages = PageSize > 0 ? (int)Math.Ceiling((double)filteredList.Count / PageSize) : 1;
            totalPages = Math.Max(1, totalPages);
            currentPage = Math.Min(currentPage, totalPages);

            Source.Clear();
            var employeesToDisplay = filteredList
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            foreach (var employee in employeesToDisplay)
            {
                Source.Add(employee);
            }

            PageInfo = $"{currentPage} / {totalPages}";
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.UpdatePagination: PageInfo = {PageInfo}, Displayed Employees = {Source.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] EmployeesViewModel.UpdatePagination: {ex.Message}");
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

        System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesViewModel.OnPageSizeChanged: PageSize changed to {value}");
        currentPage = 1;
        UpdatePagination();
    }
}