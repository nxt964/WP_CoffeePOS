using CoffeePOS.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel
    {
        get;
    }

    public DashboardPage()
    {
        ViewModel = App.GetService<DashboardViewModel>();
        InitializeComponent();
    }
}
