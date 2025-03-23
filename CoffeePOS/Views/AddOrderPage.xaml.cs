using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class AddOrderPage : Page
{
    public AddOrderViewModel ViewModel
    {
        get;
    }

    public AddOrderPage()
    {
        ViewModel = App.GetService<AddOrderViewModel>();
        InitializeComponent();
    }
}