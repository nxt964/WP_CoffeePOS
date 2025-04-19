using CoffeePOS.Core.Daos;
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
using Microsoft.UI.Xaml;

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

    [ObservableProperty]
    private bool isOrderEditable = true;

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

            var order = await _dao.Orders.GetById(orderId);
            IsOrderEditable = order?.Status != "Complete";

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
                        Price = detail.Price,
                        Image = product.Image,
                        IsEditable = IsOrderEditable
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

    private async void OrderDetailDisplay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OrderDetailDisplay.Quantity) && IsOrderEditable)
        {
            var orderDetailDisplay = sender as OrderDetailDisplay;
            if (orderDetailDisplay != null && orderDetailDisplay.IsEditable)
            {
                try
                {
                    var orderDetail = (await _dao.OrderDetails.GetAll()).FirstOrDefault(od => od.Id == orderDetailDisplay.Id);
                    if (orderDetail != null)
                    {
                        orderDetail.Quantity = orderDetailDisplay.Quantity;
                        await _dao.OrderDetails.Update(orderDetail);
                        await _dao.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.OrderDetailDisplay_PropertyChanged: Updated OrderDetail {orderDetail.Id} with Quantity = {orderDetail.Quantity}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.OrderDetailDisplay_PropertyChanged: OrderDetail {orderDetailDisplay.Id} not found");
                    }

                    CalculateTotalPrice();
                    await UpdateOrderTotalAmount();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.OrderDetailDisplay_PropertyChanged: {ex.Message}");
                }
            }
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

    [RelayCommand(CanExecute = nameof(IsOrderEditable))]
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
            await UpdateOrderTotalAmount();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.ApplyVoucher: {ex.Message}");
            VoucherInfo = "Error applying voucher.";
        }
    }

    private bool CanCheckout()
    {
        return IsOrderEditable && TotalPrice > 0;
    }

    [RelayCommand(CanExecute = nameof(CanCheckout))]
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
                var order = await _dao.Orders.GetById(orderId);
                if (order != null)
                {
                    order.Status = "Complete";
                    order.TotalAmount = TotalPrice;
                    await _dao.Orders.Update(order);
                    await _dao.SaveChangesAsync();
                    IsOrderEditable = false;
                    foreach (var detail in _allOrderDetails)
                    {
                        detail.IsEditable = false;
                    }
                    UpdateSourceWithPagination();
                    ApplyVoucherCommand.NotifyCanExecuteChanged();
                    PayCommand.NotifyCanExecuteChanged();
                    DeleteOrderDetailCommand.NotifyCanExecuteChanged();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.Pay: Order {orderId} marked as Complete, TotalAmount = {TotalPrice}");

                    // Điều hướng về trang OrderPage sau khi thanh toán thành công
                    var frame = App.MainWindow.Content as Frame;
                    frame?.Navigate(typeof(CoffeePOS.Views.OrderPage));
                }

                if (appliedVoucher != null)
                {
                    appliedVoucher.IsUsed = true;
                    await _dao.Vouchers.Update(appliedVoucher);
                    await _dao.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.Pay: Voucher {appliedVoucher.Code} marked as used");
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.Pay: Payment confirmed. TotalPrice = {TotalPrice}");
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

    [RelayCommand(CanExecute = nameof(IsOrderEditable))]
    private async Task DeleteOrderDetail(int orderDetailId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.DeleteOrderDetail: Deleting OrderDetailId = {orderDetailId}");
            await _dao.OrderDetails.Delete(orderDetailId);
            await _dao.SaveChangesAsync();
            await LoadOrderDetails();
            await UpdateOrderTotalAmount();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.DeleteOrderDetail: {ex.Message}");
        }
    }

    private async Task UpdateOrderTotalAmount()
    {
        try
        {
            var order = await _dao.Orders.GetById(orderId);
            if (order != null)
            {
                order.TotalAmount = TotalPrice;
                await _dao.Orders.Update(order);
                await _dao.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.UpdateOrderTotalAmount: Updated Order {orderId} with TotalAmount = {TotalPrice}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.UpdateOrderTotalAmount: {ex.Message}");
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
                foreach (var detail in _allOrderDetails)
                {
                    detail.IsEditable = detail.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) && IsOrderEditable;
                }
            }
            else
            {
                foreach (var detail in _allOrderDetails)
                {
                    detail.IsEditable = IsOrderEditable;
                }
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
            PayCommand.NotifyCanExecuteChanged(); // Cập nhật trạng thái của nút checkout khi tổng tiền thay đổi
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

        System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.OnPageSizeChanged: PageSize changed to {value}");
        currentPage = 1;
        UpdateSourceWithPagination();
    }
}