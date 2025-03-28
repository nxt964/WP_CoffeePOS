using CoffeePOS.Core.Daos;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoffeePOS.Views;

public sealed partial class DetailOrderPage : Page
{
    public DetailOrderViewModel ViewModel
    {
        get;
    }

    public DetailOrderPage()
    {
        this.InitializeComponent();
        ViewModel = new DetailOrderViewModel(new MockDao());
        this.DataContext = ViewModel; // Thiết lập DataContext
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int orderId)
        {
            ViewModel.OnNavigatedTo(orderId);
        }
    }

    private void AddButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Logic cho nút Add
    }

    private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag != null)
        {
            if (int.TryParse(button.Tag.ToString(), out int orderDetailId))
            {
                await ViewModel.DeleteOrderDetailCommand?.ExecuteAsync(orderDetailId);
            }
        }
    }
}