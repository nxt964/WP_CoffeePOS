using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoffeePOS.Views;

public sealed partial class AddProductToOrderDetailPage : Page
{
    public AddProductToOrderDetailViewModel ViewModel
    {
        get;
    }

    public AddProductToOrderDetailPage()
    {
        ViewModel = App.GetService<AddProductToOrderDetailViewModel>();
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.OnNavigatedTo(e.Parameter);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.OnNavigatedFrom();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddProductToOrderDetailPage.AddButton_Click: Adding products...");
        ViewModel.AddProductsCommand.Execute(Frame);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddProductToOrderDetailPage.BackButton_Click: Navigating back...");
        ViewModel.BackCommand.Execute(Frame);
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddProductToOrderDetailPage.AutoSuggestBox_TextChanged: Query = {sender.Text}");
        ViewModel.SearchQuery = sender.Text;
        ViewModel.SearchProductsCommand.Execute(null);
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddProductToOrderDetailPage.AutoSuggestBox_QuerySubmitted: Query = {args.QueryText}");
        ViewModel.SearchQuery = args.QueryText;
        ViewModel.SearchProductsCommand.Execute(null);
    }
}