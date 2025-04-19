using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class AddCustomerPage : Page
{
    public AddCustomerViewModel ViewModel
    {
        get;
    }

    public AddCustomerPage()
    {
        ViewModel = App.GetService<AddCustomerViewModel>();
        InitializeComponent();
        ViewModel.SetXamlRoot(this.XamlRoot);
    }
}