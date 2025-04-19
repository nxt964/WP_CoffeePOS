using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoffeePOS.Views;

public sealed partial class EditCustomerPage : Page
{
    public EditCustomerViewModel ViewModel
    {
        get;
    }

    public EditCustomerPage()
    {
        ViewModel = App.GetService<EditCustomerViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int customerId)
        {
            // Truyền XamlRoot vào ViewModel thông qua một phương thức khởi tạo
            ViewModel.SetXamlRoot(this.XamlRoot);
            ViewModel.OnNavigatedTo(customerId);
        }
    }
}