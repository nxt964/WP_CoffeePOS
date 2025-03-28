using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CoffeePOS.ViewModels;

public partial class UpdateProductViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private Product product = new Product();

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private BindingList<ProductType> allTypes = new BindingList<ProductType>();

    public UpdateProductViewModel()
    {
        Product = new Product();
        Message = string.Empty;
        LoadProductTypes();
    }

    public void OnNavigatedTo(object parameter)
    {
        // Giả lập dữ liệu sản phẩm hiện có (có thể thay bằng logic tải từ API hoặc database)
        Product = new Product
        {
            Id = 001,
            Name = "Sample Product",
            Price = 100.0,
            Cost = 80.0,
            Discount = 10.0,
            Manufacturer = "Sample Manufacturer",
            Quantity = 50,
            Type = "Smartphone",
            Image = "/Assets/StoreLogo.png"
        };
        Message = string.Empty;
        LoadProductTypes();
    }

    public void OnNavigatedFrom()
    {
    }

    private void LoadProductTypes()
    {
        AllTypes = new BindingList<ProductType>
        {
            new ProductType { TypeId = "1", TypeName = "Smartphone" },
            new ProductType { TypeId = "2", TypeName = "Tablet" },
            new ProductType { TypeId = "3", TypeName = "Laptop" }
        };
    }

    [RelayCommand]
    private async void LoadImage()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".gif");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            Product.Image = file.Path;
        }
    }
}