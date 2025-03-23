using CoffeePOS.Contracts.Services;
using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace CoffeePOS.ViewModels;

public partial class AddOrderViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;

    // Danh sách các tùy chọn cho ComboBox
    [ObservableProperty]
    private ObservableCollection<Customer> customers = new ObservableCollection<Customer>();

    [ObservableProperty]
    private ObservableCollection<Voucher> vouchers = new ObservableCollection<Voucher>();

    [ObservableProperty]
    private ObservableCollection<PaymentMethod> paymentMethods = new ObservableCollection<PaymentMethod>();

    [ObservableProperty]
    private ObservableCollection<ServiceType> serviceTypes = new ObservableCollection<ServiceType>();

    [ObservableProperty]
    private ObservableCollection<Table> tables = new ObservableCollection<Table>();

    // Thuộc tính để binding với các trường nhập liệu
    [ObservableProperty]
    private Customer selectedCustomer;

    [ObservableProperty]
    private Voucher selectedVoucher;

    [ObservableProperty]
    private decimal totalAmount;

    [ObservableProperty]
    private PaymentMethod selectedPaymentMethod;

    [ObservableProperty]
    private ServiceType selectedServiceType;

    [ObservableProperty]
    private Table selectedTable;

    public AddOrderViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void SaveOrder()
    {
        // Kiểm tra dữ liệu
        if (SelectedCustomer == null || TotalAmount <= 0 || SelectedPaymentMethod == null || SelectedServiceType == null || SelectedTable == null)
        {
            // Hiển thị thông báo lỗi (có thể sử dụng InfoBar hoặc ContentDialog)
            return;
        }

        // Tạo một đối tượng Order mới
        var newOrder = new Order
        {
            OrderId = Guid.NewGuid().ToString(), // Tạo ID ngẫu nhiên (thay thế bằng logic từ DB nếu cần)
            CustomerId = SelectedCustomer.Id.ToString(),
            Date = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
            VoucherId = SelectedVoucher?.Id.ToString(),
            TotalPrice = TotalAmount,
            PaymentMethodId = SelectedPaymentMethod.Id.ToString(),
            ServiceTypeId = SelectedServiceType.Id.ToString(),
            TableId = SelectedTable.Id.ToString()
        };

        // TODO: Lưu newOrder vào cơ sở dữ liệu
        // Ví dụ: Gọi một service để lưu vào DB
        // await _orderService.CreateOrderAsync(newOrder);

        // Sau khi lưu, điều hướng về trang OrderPage
        _navigationService.NavigateTo(typeof(OrderViewModel).FullName);
    }

    [RelayCommand]
    private void Cancel()
    {
        // Reset form: Xóa dữ liệu đã nhập
        SelectedCustomer = null;
        SelectedVoucher = null;
        TotalAmount = 0;
        SelectedPaymentMethod = null;
        SelectedServiceType = null;
        SelectedTable = null;
    }

}