using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class StatisticsPage : Page
{
    public StatisticsViewModel ViewModel
    {
        get;
    }

    public StatisticsPage()
    {
        ViewModel = App.GetService<StatisticsViewModel>();
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    private void DateRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.DateRangeChangedCommand != null && ViewModel.DateRangeChangedCommand.CanExecute(null))
        {
            ViewModel.DateRangeChangedCommand.Execute(null);
        }
    }

    /// <summary>
    /// Event handler for custom date pickers' date changed event
    /// </summary>
    private void CustomDate_DateChanged(object sender, DatePickerValueChangedEventArgs e)
    {
        if (ViewModel.CustomDateChangedCommand != null && ViewModel.CustomDateChangedCommand.CanExecute(null))
        {
            ViewModel.CustomDateChangedCommand.Execute(null);
        }
    }

    /// <summary>
    /// Event handler for search query submission
    /// </summary>
    private void SearchOrders_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (ViewModel.SearchOrdersCommand != null && ViewModel.SearchOrdersCommand.CanExecute(null))
        {
            ViewModel.SearchOrdersCommand.Execute(args.QueryText);
        }
    }

    /// <summary>
    /// Event handler for order filter combo box selection change
    /// </summary>
    private void OrderFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.OrderFilterChangedCommand != null && ViewModel.OrderFilterChangedCommand.CanExecute(null))
        {
            ViewModel.OrderFilterChangedCommand.Execute(null);
        }
    }

    /// <summary>
    /// Event handler for sort option combo box selection change
    /// </summary>
    private void SortOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SortOptionChangedCommand != null && ViewModel.SortOptionChangedCommand.CanExecute(null))
        {
            ViewModel.SortOptionChangedCommand.Execute(null);
        }
    }
}