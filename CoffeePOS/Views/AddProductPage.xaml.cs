using CoffeePOS.Contracts.Services;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class AddProductPage : Page
{
    public AddProductViewModel ViewModel
    {
        get;
    }

    public AddProductPage()
    {
        ViewModel = App.GetService<AddProductViewModel>();
        InitializeComponent();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(ProductViewModel).FullName);
    }
}