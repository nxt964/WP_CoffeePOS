using CoffeePOS.Core.Models;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace CoffeePOS.Views;

public sealed partial class InventoryPage : Page
{
    public InventoryViewModel ViewModel
    {
        get;
    }

    public InventoryPage()
    {
        ViewModel = App.GetService<InventoryViewModel>();
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

    private void OnIngredientSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] InventoryPage.OnIngredientSelectionChanged: Selection changed");
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Ingredient selectedIngredient)
        {
            ViewModel.LoadTransactionsCommand.Execute(selectedIngredient);
        }
    }

    private void EditIngredient_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] InventoryPage.EditIngredient_Click: Editing ingredient");
        if (sender is Button button && button.Tag is Ingredient ingredient)
        {
            ViewModel.EditIngredientCommand.Execute(ingredient);
        }
    }

    private void DeleteIngredient_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] InventoryPage.DeleteIngredient_Click: Deleting ingredient");
        if (sender is Button button && button.Tag is Ingredient ingredient)
        {
            ViewModel.DeleteIngredientCommand.Execute(ingredient);
        }
    }

    private void AddTransaction_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] InventoryPage.AddTransaction_Click: Adding transaction from grid");
        if (sender is Button button && button.Tag is Ingredient ingredient)
        {
            ViewModel.AddTransactionCommand.Execute(ingredient);
        }
    }

    private void AddTransactionSelected_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] InventoryPage.AddTransactionSelected_Click: Adding transaction for selected ingredient");
        if (ViewModel.SelectedIngredient != null)
        {
            ViewModel.AddTransactionCommand.Execute(ViewModel.SelectedIngredient);
        }
    }

    private void AddIngredient_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] InventoryPage.AddIngredient_Click: Adding new ingredient");
        ViewModel.AddIngredientCommand.Execute(null);
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Debug.WriteLine($"[DEBUG] InventoryPage.AutoSuggestBox_TextChanged: Query = {sender.Text}");
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.SearchQuery = sender.Text;
            ViewModel.SearchIngredientsCommand.Execute(null);
        }
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        Debug.WriteLine($"[DEBUG] InventoryPage.AutoSuggestBox_QuerySubmitted: Query = {args.QueryText}");
        ViewModel.SearchQuery = args.QueryText;
        ViewModel.SearchIngredientsCommand.Execute(null);
    }
}