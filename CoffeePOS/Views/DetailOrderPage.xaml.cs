using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class DetailOrderPage : Page
{
    public DetailOrderViewModel ViewModel
    {
        get;
    }

    public DetailOrderPage()
    {
        ViewModel = App.GetService<DetailOrderViewModel>();
        InitializeComponent();
    }
}