using CoffeePOS.Contracts.Services;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class ProductPage : Page
{
    public ProductViewModel ViewModel { get; }

    public ProductPage()
    {
        ViewModel = App.GetService<ProductViewModel>();
        InitializeComponent();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(AddProductViewModel).FullName);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var productId = button?.Tag?.ToString();
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(UpdateProductViewModel).FullName, productId);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var productId = button?.Tag?.ToString();
        if (!string.IsNullOrEmpty(productId))
        {
            ViewModel.DeleteProduct(productId);
        }
    }
}