using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CoffeePOS.Core.Models;
using System.Diagnostics;
namespace CoffeePOS.Views;

public sealed partial class TablePage : Page
{
    public TableViewModel ViewModel
    {
        get;
    }

    public TablePage()
    {
        ViewModel = App.GetService<TableViewModel>();
        InitializeComponent();
        this.DataContext = ViewModel;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Table table)
        {
            ViewModel.EditTableCommand.Execute(table);
        }
    }
    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Table table)
        {
            ViewModel.DeleteTableCommand.Execute(table);
        }
    }
    private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Table table)
        {
            ViewModel.ChangeStatusCommand.Execute(table);
        }
    }
}