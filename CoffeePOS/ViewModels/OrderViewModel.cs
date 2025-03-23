using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace CoffeePOS.ViewModels;

public partial class OrderViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private ObservableCollection<Order> source = new ObservableCollection<Order>();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private DateTimeOffset fromDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset toDate = DateTimeOffset.Now;

    [ObservableProperty]
    private int pageSize = 10;

    [ObservableProperty]
    private string pageInfo = "1 / 1";

    private int currentPage = 1;
    private int totalPages = 1;

    public OrderViewModel()
    {
        // Không cần INavigationService nữa
    }

    public void OnNavigatedTo(object parameter)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.OnNavigatedTo: Loading orders...");
        Source.Clear();
        Source.Add(new Order { OrderId = "1", Date = "2025-03-23", CustomerId = "1", VoucherId = "1", PaymentMethodId = "1", ServiceTypeId = "1", TableId = "1", TotalPrice = 100.50m });
        Source.Add(new Order { OrderId = "2", Date = "2025-03-24", CustomerId = "2", VoucherId = "2", PaymentMethodId = "2", ServiceTypeId = "2", TableId = "2", TotalPrice = 200.75m });

        UpdatePagination();
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.OnNavigatedFrom: Leaving OrderPage...");
    }

    [RelayCommand]
    private void Add()
    {
        // Chuyển logic điều hướng sang OrderPage.xaml.cs, nên phương thức này sẽ trống
        // Nếu cần, bạn có thể để lại một sự kiện hoặc thông báo để OrderPage.xaml.cs xử lý
    }

    [RelayCommand]
    private void Delete(string orderId)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Delete: Deleting OrderId = {orderId}");
        var order = Source.FirstOrDefault(o => o.OrderId == orderId);
        if (order != null)
        {
            Source.Remove(order);
        }
        UpdatePagination();
    }

    [RelayCommand]
    private void Search()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Search: SearchQuery = {SearchQuery}");
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void Filter()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.Filter: FromDate = {FromDate}, ToDate = {ToDate}");
        currentPage = 1;
        UpdatePagination();
    }

    [RelayCommand]
    private void RemoveFilter()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderViewModel.RemoveFilter: Resetting filters...");
        FromDate = DateTimeOffset.Now;
        ToDate = DateTimeOffset.Now;
        SearchQuery = null;
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.PreviousPage: CurrentPage = {currentPage}");
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            UpdatePagination();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.NextPage: CurrentPage = {currentPage}");
        }
    }

    public void UpdatePagination()
    {
        totalPages = (int)Math.Ceiling((double)Source.Count / PageSize);
        totalPages = Math.Max(1, totalPages);
        currentPage = Math.Min(currentPage, totalPages);
        PageInfo = $"{currentPage} / {totalPages}";
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderViewModel.UpdatePagination: PageInfo = {PageInfo}");
    }
}