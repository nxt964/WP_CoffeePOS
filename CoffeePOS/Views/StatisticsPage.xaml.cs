using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Syncfusion.UI.Xaml.Charts;

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

        // Set up event handlers for property changes
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update chart title when data type changes
        if (e.PropertyName == nameof(ViewModel.DataType))
        {
            UpdateChartTitle();
        }
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

    private void PreviousPeriod_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.PreviousPeriodCommand.Execute(null);
        UpdatePeriodDisplay();
    }

    private void NextPeriod_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NextPeriodCommand.Execute(null);
        UpdatePeriodDisplay();
    }

    private void TimeFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && sender is ComboBox comboBox)
        {
            string selectedMode = (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Year";
            ViewModel.TimePeriodMode = selectedMode;
            ViewModel.UpdateTimePeriodCommand.Execute(null);
            UpdatePeriodDisplay();
        }
    }

    private void DataType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && sender is ComboBox comboBox)
        {
            string selectedType = (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Revenue";
            ViewModel.ChangeDataTypeCommand.Execute(selectedType);
            UpdateChartTitle();
        }
    }

    private void OrderStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && sender is ComboBox comboBox)
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

    private void UpdatePeriodDisplay()
    {
        // Update the period display text
        PeriodDisplay.Text = ViewModel.PeriodDisplayText;
    }

    private void UpdateChartTitle()
    {
        // Check if elements are initialized before using them
        if (ChartTitle == null || YAxis == null)
            return;

        // Update chart title based on the selected data type
        if (ViewModel.DataType == "Revenue")
        {
            ChartTitle.Text = "Revenue Trend";
            YAxis.Header = "Revenue ($)";

            // Check if ColumnSeries exists before updating its properties
            if (ColumnSeries != null)
                ColumnSeries.Label = "Revenue";
        }
        else
        {
            ChartTitle.Text = "Profit Trend";
            YAxis.Header = "Profit ($)";

            // Check if ColumnSeries exists before updating its properties
            if (ColumnSeries != null)
                ColumnSeries.Label = "Profit";
        }
    }



    private void DistributionType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null || DistributionPieChart == null || PieChartTitle == null)
            return;

        if (sender is ComboBox comboBox)
        {
            int selectedIndex = comboBox.SelectedIndex;

            // Update chart title and data source
            if (selectedIndex == 0) // Service Type
            {
                PieChartTitle.Text = "Distribution by Service Type";

                // Get the PieSeries from the chart and update its bindings
                var pieSeries = DistributionPieChart.Series.FirstOrDefault() as Syncfusion.UI.Xaml.Charts.PieSeries;
                if (pieSeries != null)
                {
                    pieSeries.ItemsSource = ViewModel.ServiceTypeDistribution;
                    pieSeries.XBindingPath = "ServiceTypeName";
                    pieSeries.YBindingPath = "TotalSales";
                }
            }
            else // Payment Method
            {
                PieChartTitle.Text = "Distribution by Payment Method";

                // Get the PieSeries from the chart and update its bindings
                var pieSeries = DistributionPieChart.Series.FirstOrDefault() as Syncfusion.UI.Xaml.Charts.PieSeries;
                if (pieSeries != null)
                {
                    pieSeries.ItemsSource = ViewModel.PaymentMethodDistribution;
                    pieSeries.XBindingPath = "PaymentMethodName";
                    pieSeries.YBindingPath = "TotalSales";
                }
            }
        }
    }
}