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
    private readonly IDao _dao;

    public DetailOrderPage()
    {
        _dao = new MockDao();
        ViewModel = App.GetService<DetailOrderViewModel>();
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int orderId)
        {
            ViewModel.OnNavigatedTo(orderId);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Check if navigating to AddProductToOrderDetailPage
        if (e.SourcePageType == typeof(AddProductToOrderDetailPage))
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] DetailOrderPage.OnNavigatedFrom: Navigating to AddProductToOrderDetailPage, will refresh on return");
            // Register a handler to refresh when navigating back
            Frame.Navigated += Frame_Navigated;
        }
    }

    private void Frame_Navigated(object sender, NavigationEventArgs e)
    {
        // Check if navigating back to this page
        if (e.SourcePageType == typeof(DetailOrderPage))
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] DetailOrderPage.Frame_Navigated: Navigated back to DetailOrderPage, refreshing order details");
            ViewModel.RefreshCommand.Execute(null);
        }
        // Unsubscribe to avoid multiple handlers
        Frame.Navigated -= Frame_Navigated;
    }

    private void AddButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] DetailOrderPage.AddButton_Click: Navigating to AddProductToOrderDetailPage");
        Frame.Navigate(typeof(AddProductToOrderDetailPage), (ViewModel.OrderId, _dao));
    }

    private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag != null)
        {
            if (int.TryParse(button.Tag.ToString(), out int orderDetailId))
            {
                await ViewModel.DeleteOrderDetailCommand.ExecuteAsync(orderDetailId);
            }
        }
    }

    private void PageSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Try to parse the input as an integer
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                // If the input is empty, set a default value (e.g., 1)
                textBox.Text = "1";
                textBox.SelectAll();
                return;
            }

            if (!int.TryParse(textBox.Text, out int newPageSize) || newPageSize < 1)
            {
                // If the input is not a valid integer or is less than 1, revert to the previous valid value
                textBox.Text = ViewModel.PageSize.ToString();
                textBox.SelectAll();
            }
        }
    }
}