using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class AddEmployeePage : Page
{
    public AddEmployeeViewModel ViewModel
    {
        get;
    }

    public AddEmployeePage()
    {
        ViewModel = App.GetService<AddEmployeeViewModel>();
        InitializeComponent();
    }
}