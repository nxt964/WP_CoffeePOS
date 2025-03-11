using CoffeePOS.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
