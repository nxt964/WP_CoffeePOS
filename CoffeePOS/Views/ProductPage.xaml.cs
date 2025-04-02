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
        if (sender is Button button && button.Tag is int productId)
        {
            var navigationService = App.GetService<INavigationService>();
            navigationService.NavigateTo(typeof(UpdateProductViewModel).FullName, productId);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int productId)
        {
            ViewModel.DeleteProduct(productId);
        }
    }

}