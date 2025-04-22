using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

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

    private void DateRange_Changed(object sender, DatePickerValueChangedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] StatisticsPage.DateRange_Changed: Date range changed");
        // No need to update ViewModel properties as they are bound to the DatePicker.Date property
        // Just reload the data
        ViewModel.LoadStatisticsCommand.Execute(null);
    }

    private void OrderStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] StatisticsPage.OrderStatusFilter_SelectionChanged: Status filter changed");
        if (sender is ComboBox comboBox)
        {
            string statusFilter = "All";

            if (comboBox.SelectedIndex > 0)
            {
                statusFilter = (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All";
            }

            ViewModel.OrderStatusFilter = statusFilter;
            ViewModel.FilterOrdersCommand.Execute(null);
        }
    }
}