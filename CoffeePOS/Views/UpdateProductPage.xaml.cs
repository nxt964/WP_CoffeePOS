using CoffeePOS.Contracts.Services;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class UpdateProductPage : Page
{
    public UpdateProductViewModel ViewModel
    {
        get;
    }

    public UpdateProductPage()
    {
        ViewModel = App.GetService<UpdateProductViewModel>();
        InitializeComponent();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(ProductViewModel).FullName);
    }
}