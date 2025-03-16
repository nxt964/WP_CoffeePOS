using CoffeePOS.Services;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace CoffeePOS.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }
    private readonly FirebaseService _firebaseService;

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _firebaseService = new FirebaseService();
        InitializeComponent();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Sample order data
        var order = new
        {
            OrderId = 1,
            Item = "Coffee",
            Price = 3.99,
            Timestamp = DateTime.UtcNow.ToString("o")
        };

        // Save to Firebase under the "orders/order1" path
        bool success = await _firebaseService.WriteDataAsync("orders/order1", order);
        ResultText.Text = success ? "Order saved successfully!" : "Failed to save order.";
    }

    private async void ReadButton_Click(object sender, RoutedEventArgs e)
    {
        // Read the order from Firebase
        var order = await _firebaseService.ReadDataAsync<dynamic>("orders/order1");
        if (order != null)
        {
            ResultText.Text = $"Order: {order.Item}, Price: {order.Price}";
        }
        else
        {
            ResultText.Text = "Failed to read order.";
        }
    }
}