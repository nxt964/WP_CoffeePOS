using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class TableDetailPage : Page
{
    public TableDetailViewModel ViewModel
    {
        get;
    }

    public TableDetailPage()
    {
        ViewModel = App.GetService<TableDetailViewModel>();
        InitializeComponent();
    }
}