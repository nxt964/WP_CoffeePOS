using CoffeePOS.Contracts.Services;
using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace CoffeePOS.ViewModels;

public partial class DetailOrderViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Order order;

    [ObservableProperty]
    private string customerName;

    [ObservableProperty]
    private string voucherCode;

    [ObservableProperty]
    private string paymentMethodName;

    [ObservableProperty]
    private string serviceTypeName;

    [ObservableProperty]
    private string tableName;

    public DetailOrderViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is string orderId)
        {
            // Giả lập dữ liệu đơn hàng (thay thế bằng logic lấy từ DB nếu cần)
            order = new Order
            {
                OrderId = orderId,
                CustomerId = "1",
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                VoucherId = "1",
                TotalPrice = 100.50m,
                PaymentMethodId = "1",
                ServiceTypeId = "1",
                TableId = "1"
            };

            // Giả lập dữ liệu liên quan (thay thế bằng logic lấy từ DB nếu cần)
            customerName = "Customer 1";
            voucherCode = Order.VoucherId != null ? "VOUCHER001" : "None";
            paymentMethodName = "Cash";
            serviceTypeName = "Dine In";
            tableName = "Table 1";
        }
    }

    public void OnNavigatedFrom()
    {
        // Dọn dẹp nếu cần
    }

    [RelayCommand]
    private void Back()
    {
        // Quay lại trang OrderPage
        _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
    }
}