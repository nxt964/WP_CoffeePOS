using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CoffeePOS.ViewModels;

public partial class AddProductViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private Product product = new Product();

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private BindingList<ProductType> allTypes = new BindingList<ProductType>();

    public AddProductViewModel()
    {
        Product = new Product();
        Message = string.Empty;
        LoadProductTypes();
    }

    public void OnNavigatedTo(object parameter)
    {
        Product = new Product();
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