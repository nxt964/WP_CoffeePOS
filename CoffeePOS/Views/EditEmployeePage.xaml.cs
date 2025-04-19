using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoffeePOS.Views;

public sealed partial class EditEmployeePage : Page
{
    public EditEmployeeViewModel ViewModel
    {
        get;
    }

    public EditEmployeePage()
    {
        ViewModel = App.GetService<EditEmployeeViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int employeeId)
        {
            ViewModel.OnNavigatedTo(employeeId);
        }
    }
}