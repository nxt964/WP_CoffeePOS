﻿using CoffeePOS.Core.Daos;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.ViewModels;

public partial class DetailOrderViewModel : ObservableObject
{
    private readonly IDao _dao;
    private ObservableCollection<OrderDetailDisplay> _allOrderDetails = new();

    [ObservableProperty]
    private ObservableCollection<OrderDetailDisplay> source = new();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private int pageSize = 5;

    [ObservableProperty]
    private string pageInfo = "1 / 1";

    [ObservableProperty]
    private string voucherCode;

    [ObservableProperty]
    private decimal totalPrice;

    [ObservableProperty]
    private string voucherInfo;

    private int currentPage = 1;
    private int totalPages = 1;
    private int orderId;
    private Voucher appliedVoucher;

    public int OrderId => orderId;

    public DetailOrderViewModel(IDao dao)
    {
        _dao = dao;
    }

    public async void OnNavigatedTo(int orderId)
    {
        this.orderId = orderId;
        await LoadOrderDetails();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadOrderDetails();
    }

    private async Task LoadOrderDetails()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.LoadOrderDetails: Loading details for OrderId = {orderId}");
            _allOrderDetails.Clear();
            Source.Clear();

            var orderDetails = (await _dao.OrderDetails.GetAll()).Where(od => od.OrderId == orderId).ToList();
            var products = await _dao.Products.GetAll();

            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.LoadOrderDetails: Found {orderDetails.Count} order details for OrderId = {orderId}");

            foreach (var detail in orderDetails)
            {
                var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                if (product != null)
                {
                    var orderDetailDisplay = new OrderDetailDisplay
                    {
                        Id = detail.Id,
                        ProductId = detail.ProductId,
                        ProductName = product.Name,
                        Quantity = detail.Quantity,
                        Price = detail.Price
                    };
                    orderDetailDisplay.PropertyChanged += OrderDetailDisplay_PropertyChanged;
                    _allOrderDetails.Add(orderDetailDisplay);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.LoadOrderDetails: Loaded {_allOrderDetails.Count} items into _allOrderDetails");

            UpdateSourceWithPagination();
            CalculateTotalPrice();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.LoadOrderDetails: {ex.Message}");
            throw;
        }
    }

    private void OrderDetailDisplay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OrderDetailDisplay.Quantity))
        {
            CalculateTotalPrice();
        }
    }

    [RelayCommand]
    private void Search()
    {
        try
        {
            currentPage = 1;
            UpdateSourceWithPagination();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.Search: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ApplyVoucher()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(VoucherCode))
            {
                var vouchers = await _dao.Vouchers.GetAll();
                var voucher = vouchers.FirstOrDefault(v => v.Code == VoucherCode && !v.IsUsed && v.ExpirationDate >= DateTime.Now);
                appliedVoucher = voucher;
                if (voucher != null)
                {
                    VoucherInfo = $"Voucher Applied {voucher.Code}: Discount {voucher.DiscountPercentage}%";
                }
                else
                {
                    VoucherInfo = "Invalid or expired voucher.";
                }
            }
            else
            {
                appliedVoucher = null;
                VoucherInfo = null;
            }
            CalculateTotalPrice();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.ApplyVoucher: {ex.Message}");
            VoucherInfo = "Error applying voucher.";
        }
    }

    [RelayCommand]
    private async Task Pay()
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Payment",
                CloseButtonText = "Cancel",
                Content = $"Total Price: {TotalPrice:C}. Do you want to proceed?",
                PrimaryButtonText = "Pay",
            };

            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Logic thanh toán: cập nhật trạng thái đơn hàng, đánh dấu voucher đã sử dụng, v.v.
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.Pay: Payment confirmed. TotalPrice = {TotalPrice}");
                TotalPrice += 1.00m; // Tăng TotalPrice để kiểm tra binding
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.Pay: TotalPrice updated to {TotalPrice}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.Pay: {ex.Message}");
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        try
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateSourceWithPagination();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.PreviousPage: {ex.Message}");
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        try
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdateSourceWithPagination();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.NextPage: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteOrderDetail(int orderDetailId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.DeleteOrderDetail: Deleting OrderDetailId = {orderDetailId}");
            await _dao.OrderDetails.Delete(orderDetailId);
            await _dao.SaveChangesAsync();
            await LoadOrderDetails();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.DeleteOrderDetail: {ex.Message}");
        }
    }

    private void UpdateSourceWithPagination()
    {
        try
        {
            var filteredDetails = _allOrderDetails.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filteredDetails = filteredDetails.Where(d => d.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            var filteredList = filteredDetails.ToList();
            totalPages = (int)Math.Ceiling((double)filteredList.Count / PageSize);
            totalPages = Math.Max(1, totalPages);
            currentPage = Math.Min(currentPage, totalPages);

            Source.Clear();
            var pageItems = filteredList
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var item in pageItems)
            {
                Source.Add(item);
            }

            PageInfo = $"{currentPage} / {totalPages}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.UpdateSourceWithPagination: {ex.Message}");
        }
    }

    private void CalculateTotalPrice()
    {
        try
        {
            decimal total = _allOrderDetails.Sum(od => od.Price * od.Quantity);
            if (appliedVoucher != null)
            {
                total -= total * (appliedVoucher.DiscountPercentage / 100);
            }
            TotalPrice = total;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.CalculateTotalPrice: TotalPrice = {TotalPrice}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.CalculateTotalPrice: {ex.Message}");
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        Search();
    }

    // Add handler for PageSize changes
    partial void OnPageSizeChanged(int value)
    {
        // Enforce minimum and maximum PageSize
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

        System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.OnPageSizeChanged: PageSize changed to {value}");
        currentPage = 1; // Reset to the first page when PageSize changes
        UpdateSourceWithPagination();
    }
}