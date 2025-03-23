using CoffeePOS.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class TablePage : Page
{
    public TableViewModel ViewModel
    {
        get;
    }

    public TablePage()
    {
        ViewModel = App.GetService<TableViewModel>();
        InitializeComponent();
    }
}
