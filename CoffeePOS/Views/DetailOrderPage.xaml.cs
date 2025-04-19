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

        // Nếu điều hướng về OrderPage, làm mới OrderViewModel
        if (e.SourcePageType == typeof(OrderPage))
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] DetailOrderPage.OnNavigatedFrom: Navigating back to OrderPage, refreshing OrderViewModel");
            var orderViewModel = App.GetService<OrderViewModel>();
            orderViewModel.RefreshCommand.Execute(null);
        }
        // Nếu điều hướng đến AddProductToOrderDetailPage, làm mới khi quay lại
        else if (e.SourcePageType == typeof(AddProductToOrderDetailPage))
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] DetailOrderPage.OnNavigatedFrom: Navigating to AddProductToOrderDetailPage, will refresh on return");
            Frame.Navigated += Frame_Navigated;
        }
    }

    private void Frame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.SourcePageType == typeof(DetailOrderPage))
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] DetailOrderPage.Frame_Navigated: Navigated back to DetailOrderPage, refreshing order details");
            ViewModel.RefreshCommand.Execute(null);
        }
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
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "1";
                textBox.SelectAll();
                return;
            }

            if (!int.TryParse(textBox.Text, out int newPageSize) || newPageSize < 1)
            {
                textBox.Text = ViewModel.PageSize.ToString();
                textBox.SelectAll();
            }
        }
    }
}