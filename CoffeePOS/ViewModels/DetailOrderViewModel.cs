using CoffeePOS.Core.Daos;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;

// Aliases to avoid ambiguity
using CorePaymentMethod = CoffeePOS.Core.Models.PaymentMethod;
using UIPaymentMethod = CoffeePOS.Models.PaymentMethodDisplay;

namespace CoffeePOS.ViewModels;

public partial class DetailOrderViewModel : ObservableObject
{
    private readonly IDao _dao;
    private ObservableCollection<OrderDetailDisplay> _allOrderDetails = new();

    public class PaymentMethodItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public UIPaymentMethod PaymentMethod
        {
            get;
        }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PaymentMethodItem(UIPaymentMethod paymentMethod)
        {
            PaymentMethod = paymentMethod;
            _isSelected = false;
        }
    }

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

    [ObservableProperty]
    private ObservableCollection<PaymentMethodItem> paymentMethodItems = new();

    [ObservableProperty]
    private CorePaymentMethod selectedPaymentMethod;

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
        try
        {
            this.orderId = orderId;
            await LoadOrderDetails();
            await LoadPaymentMethods();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.OnNavigatedTo: Failed to initialize page for OrderId = {orderId}. Error: {ex.Message}");
            await ShowErrorDialogAsync("Failed to load order details. Please try again.", null);
        }
    }

    private async Task LoadPaymentMethods()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] DetailOrderViewModel.LoadPaymentMethods: Loading payment methods...");
            PaymentMethodItems.Clear();
            var coreMethods = await _dao.PaymentMethods.GetAll();
            if (coreMethods == null)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] DetailOrderViewModel.LoadPaymentMethods: Payment methods data is null.");
                return;
            }

            foreach (var coreMethod in coreMethods)
            {
                if (coreMethod != null)
                {
                    var uiMethod = new UIPaymentMethod
                    {
                        Id = coreMethod.Id,
                        Name = coreMethod.Name
                    };
                    var item = new PaymentMethodItem(uiMethod);
                    item.PropertyChanged += PaymentMethodItem_PropertyChanged;
                    PaymentMethodItems.Add(item);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.LoadPaymentMethods: Loaded {PaymentMethodItems.Count} payment methods");

            if (PaymentMethodItems.Any())
            {
                var firstItem = PaymentMethodItems.First();
                firstItem.IsSelected = true; // Set default
                SelectedPaymentMethod = coreMethods.FirstOrDefault(m => m.Id == firstItem.PaymentMethod.Id);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.LoadPaymentMethods: Set default SelectedPaymentMethod to {SelectedPaymentMethod?.Name} (Id: {SelectedPaymentMethod?.Id})");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.LoadPaymentMethods: {ex.Message}");
        }
    }

    private void PaymentMethodItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PaymentMethodItem.IsSelected))
        {
            var item = (PaymentMethodItem)sender;
            if (item.IsSelected)
            {
                // Unselect others
                foreach (var other in PaymentMethodItems.Where(i => !i.Equals(item)))
                {
                    other.IsSelected = false;
                }
                SelectedPaymentMethod = new CorePaymentMethod
                {
                    Id = item.PaymentMethod.Id,
                    Name = item.PaymentMethod.Name
                };
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.PaymentMethodItem_PropertyChanged: Selected PaymentMethod = {SelectedPaymentMethod?.Name} (Id: {SelectedPaymentMethod?.Id})");
                PayCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            await LoadOrderDetails();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.Refresh: {ex.Message}");
            await ShowErrorDialogAsync("Failed to refresh order details.", null);
        }
    }

    private async Task LoadOrderDetails()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.LoadOrderDetails: Loading details for OrderId = {orderId}");
            _allOrderDetails.Clear();
            Source.Clear();

            var order = await _dao.Orders.GetById(orderId);
            if (order == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.LoadOrderDetails: Order with ID {orderId} not found.");
                return;
            }

            IsOrderEditable = order.Status != "Complete";

            var orderDetails = (await _dao.OrderDetails.GetAll())?.Where(od => od.OrderId == orderId).ToList();
            if (orderDetails == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.LoadOrderDetails: OrderDetails is null.");
                return;
            }

            var products = await _dao.Products.GetAll();
            if (products == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.LoadOrderDetails: Products data is null.");
                return;
            }

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
            await CalculateTotalPrice();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.LoadOrderDetails: {ex.Message}");
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
                    var orderDetail = (await _dao.OrderDetails.GetAll())?.FirstOrDefault(od => od.Id == orderDetailDisplay.Id);
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

                    await CalculateTotalPrice();
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
                var voucher = vouchers?.FirstOrDefault(v => v.Code == VoucherCode);
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
            await CalculateTotalPrice();
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
        return IsOrderEditable && TotalPrice > 0 && SelectedPaymentMethod != null;
    }

    [RelayCommand(CanExecute = nameof(CanCheckout))]
    private async Task Pay(Frame frame)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Payment",
                CloseButtonText = "Cancel",
                Content = $"Total Price: {TotalPrice:C}. Payment Method: {SelectedPaymentMethod?.Name ?? "None"}. Do you want to proceed?",
                PrimaryButtonText = "Pay",
                XamlRoot = frame?.XamlRoot ?? App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var order = await _dao.Orders.GetById(orderId);
                if (order != null)
                {
                    order.Status = "Complete";
                    order.TotalAmount = TotalPrice;
                    order.PaymentMethodId = SelectedPaymentMethod?.Id;
                    await _dao.Orders.Update(order);
                    await _dao.SaveChangesAsync();

                    if (order.CustomerId.HasValue)
                    {
                        var customer = await _dao.Customers.GetById(order.CustomerId.Value);
                        if (customer != null)
                        {
                            int pointsToAdd = (int)TotalPrice;
                            customer.Points += pointsToAdd;
                            customer.IsMembership = customer.Points >= 100;
                            await _dao.Customers.Update(customer);
                            await _dao.SaveChangesAsync();
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.Pay: Added {pointsToAdd} points to Customer {customer.Id}. New Points: {customer.Points}, IsMembership: {customer.IsMembership}");
                        }
                    }

                    IsOrderEditable = false;
                    foreach (var detail in _allOrderDetails)
                    {
                        detail.IsEditable = false;
                    }
                    UpdateSourceWithPagination();
                    ApplyVoucherCommand.NotifyCanExecuteChanged();
                    PayCommand.NotifyCanExecuteChanged();
                    DeleteOrderDetailCommand.NotifyCanExecuteChanged();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.Pay: Order {orderId} marked as Complete, TotalAmount = {TotalPrice}, PaymentMethodId = {order.PaymentMethodId}");

                    frame?.Navigate(typeof(CoffeePOS.Views.OrderPage));
                }

                if (appliedVoucher != null)
                {
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
            await ShowErrorDialogAsync("Failed to process payment.", frame?.XamlRoot);
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
            await ShowErrorDialogAsync("Failed to delete order detail.", null);
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

    private async Task CalculateTotalPrice()
    {
        try
        {
            decimal total = _allOrderDetails.Sum(od => od.Price * od.Quantity);
            if (appliedVoucher != null)
            {
                total -= total * (appliedVoucher.DiscountPercentage / 100);
            }

            var order = await _dao.Orders.GetById(orderId);
            if (order != null && order.CustomerId.HasValue)
            {
                var customer = await _dao.Customers.GetById(order.CustomerId.Value);
                if (customer != null && customer.IsMembership)
                {
                    total -= total * 0.05m;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.CalculateTotalPrice: Applied 5% membership discount for Customer {customer.Id}. New Total: {total}");
                }
            }

            TotalPrice = total;
            PayCommand.NotifyCanExecuteChanged();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.CalculateTotalPrice: TotalPrice = {TotalPrice}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.CalculateTotalPrice: {ex.Message}");
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

    private async Task ShowErrorDialogAsync(string message, XamlRoot xamlRoot)
    {
        var tcs = new TaskCompletionSource<bool>();
        var enqueueResult = DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
        {
            try
            {
                xamlRoot = xamlRoot ?? App.MainWindow.Content.XamlRoot;
                if (xamlRoot == null)
                {
                    System.Diagnostics.Debug.WriteLine("[ERROR] DetailOrderViewModel.ShowErrorDialogAsync: xamlRoot is null, cannot show dialog");
                    tcs.SetResult(false);
                    return;
                }

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = xamlRoot
                };
                await dialog.ShowAsync();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] DetailOrderViewModel.ShowErrorDialogAsync: Failed to show dialog. Exception: {ex.Message}");
                tcs.SetResult(false);
            }
        });

        System.Diagnostics.Debug.WriteLine($"[DEBUG] DetailOrderViewModel.ShowErrorDialogAsync: DispatcherQueue.TryEnqueue result: {enqueueResult}");
        if (!enqueueResult)
        {
            System.Diagnostics.Debug.WriteLine("[ERROR] DetailOrderViewModel.ShowErrorDialogAsync: Failed to enqueue dialog creation on UI thread");
            tcs.SetResult(false);
        }

        await tcs.Task;
    }
}