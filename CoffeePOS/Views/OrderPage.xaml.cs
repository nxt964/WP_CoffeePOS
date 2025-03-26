using CoffeePOS.Core.Interfaces;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class OrderPage : Page
{
    public OrderViewModel ViewModel { get; }

    public OrderPage()
    {
        ViewModel = App.GetService<OrderViewModel>();
        InitializeComponent();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] OrderPage.AddButton_Click: Navigating to NewOrderPage...");
        Frame.Navigate(typeof(AddOrderPage));
    }

    private void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        //var button = sender as Button;
        //var orderId = button?.Tag?.ToString();
        //System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderPage.ViewButton_Click: Navigating to DetailOrderPage with OrderId = {orderId}");
        //Frame.Navigate(typeof(DetailOrderPage), orderId);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var orderId = button?.Tag?.ToString();
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OrderPage.DeleteButton_Click: Deleting OrderId = {orderId}");
        ViewModel.DeleteCommand.Execute(orderId);
    }
}