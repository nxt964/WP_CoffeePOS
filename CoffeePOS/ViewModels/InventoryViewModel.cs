using System.Collections.ObjectModel;
using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Contracts.Services;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Helpers;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.ViewModels;

public partial class InventoryViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;
    private readonly ContentDialogHelper _contentDialogHelper;

    [ObservableProperty]
    private ObservableCollection<Ingredient> _ingredients = new();

    [ObservableProperty]
    private ObservableCollection<IngredientInventoryTransaction> _transactions = new();

    [ObservableProperty]
    private Ingredient? _selectedIngredient;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public InventoryViewModel(IDao dao)
    {
        _dao = dao;
        _contentDialogHelper = new ContentDialogHelper();
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadIngredientsAsync();
    }

    public void OnNavigatedFrom()
    {
    }

    public async Task LoadIngredientsAsync()
    {
        var ingredients = await _dao.Ingredients.GetAll();
        Ingredients.Clear();
        foreach (var ingredient in ingredients)
        {
            Ingredients.Add(ingredient);
        }
    }

    [RelayCommand]
    private async Task LoadTransactionsAsync(Ingredient ingredient)
    {
        SelectedIngredient = ingredient;
        var transactions = await _dao.IngredientInventoryTransactions.GetAll();
        Transactions.Clear();
        foreach (var transaction in transactions.Where(t => t.IngredientId == ingredient.Id))
        {
            Transactions.Add(transaction);
        }
    }

    [RelayCommand]
    private async Task SearchIngredients()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadIngredientsAsync();
            return;
        }

        var allIngredients = await _dao.Ingredients.GetAll();
        var filteredIngredients = allIngredients.Where(i =>
            i.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
            i.Unit.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        Ingredients.Clear();
        foreach (var ingredient in filteredIngredients)
        {
            Ingredients.Add(ingredient);
        }
    }

    [RelayCommand]
    private async Task AddIngredient()
    {
        var contentPanel = new StackPanel { Spacing = 10 };

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Name:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "NameBox", PlaceholderText = "Ingredient Name", Width = 200 }
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Unit:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "UnitBox", PlaceholderText = "Unit (e.g. bag, liter)", Width = 200 }
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Quantity:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new NumberBox { Name = "QuantityBox", PlaceholderText = "Initial Quantity", Value = 0, Minimum = 0, Width = 200 }
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Threshold:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new NumberBox { Name = "ThresholdBox", PlaceholderText = "Alert Threshold", Value = 0, Minimum = 0, Width = 200 }
            }
        });

        var dialog = new ContentDialog
        {
            Title = "Add New Ingredient",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = contentPanel,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await _contentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                var nameBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<TextBox>().FirstOrDefault()?.Name == "NameBox")?.Children.OfType<TextBox>().FirstOrDefault();
                var unitBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<TextBox>().FirstOrDefault()?.Name == "UnitBox")?.Children.OfType<TextBox>().FirstOrDefault();
                var quantityBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<NumberBox>().FirstOrDefault()?.Name == "QuantityBox")?.Children.OfType<NumberBox>().FirstOrDefault();
                var thresholdBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<NumberBox>().FirstOrDefault()?.Name == "ThresholdBox")?.Children.OfType<NumberBox>().FirstOrDefault();

                if (string.IsNullOrWhiteSpace(nameBox?.Text) || string.IsNullOrWhiteSpace(unitBox?.Text))
                {
                    await ShowErrorMessage("Name and Unit cannot be empty");
                    return;
                }

                var newIngredient = new Ingredient
                {
                    Name = nameBox.Text,
                    Unit = unitBox.Text,
                    Quantity = (int)(quantityBox?.Value ?? 0),
                    Threshold = (int)(thresholdBox?.Value ?? 0)
                };

                await _dao.Ingredients.Add(newIngredient);

                // Add initial transaction if quantity > 0
                if (newIngredient.Quantity > 0)
                {
                    var initialTransaction = new IngredientInventoryTransaction
                    {
                        IngredientId = newIngredient.Id,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                        Quantity = newIngredient.Quantity,
                        Unit = newIngredient.Unit,
                        TransactionType = "IMPORT",
                        UnitPrice = 0.0 // No price for initial setup
                    };

                    await _dao.IngredientInventoryTransactions.Add(initialTransaction);
                }

                await LoadIngredientsAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Error adding ingredient: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task EditIngredient(Ingredient ingredient)
    {
        var contentPanel = new StackPanel { Spacing = 10 };

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Name:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "NameBox", Text = ingredient.Name, Width = 200 }
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Unit:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "UnitBox", Text = ingredient.Unit, Width = 200 }
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Threshold:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new NumberBox { Name = "ThresholdBox", Value = ingredient.Threshold, Minimum = 0, Width = 200 }
            }
        });

        var dialog = new ContentDialog
        {
            Title = "Edit Ingredient",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = contentPanel,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await _contentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                var nameBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<TextBox>().FirstOrDefault()?.Name == "NameBox")?.Children.OfType<TextBox>().FirstOrDefault();
                var unitBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<TextBox>().FirstOrDefault()?.Name == "UnitBox")?.Children.OfType<TextBox>().FirstOrDefault();
                var thresholdBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<NumberBox>().FirstOrDefault()?.Name == "ThresholdBox")?.Children.OfType<NumberBox>().FirstOrDefault();

                if (string.IsNullOrWhiteSpace(nameBox?.Text) || string.IsNullOrWhiteSpace(unitBox?.Text))
                {
                    await ShowErrorMessage("Name and Unit cannot be empty");
                    return;
                }

                var updatedIngredient = new Ingredient
                {
                    Id = ingredient.Id,
                    Name = nameBox.Text,
                    Unit = unitBox.Text,
                    Quantity = ingredient.Quantity, // Keep the same quantity
                    Threshold = (int)(thresholdBox?.Value ?? 0)
                };

                await _dao.Ingredients.Update(updatedIngredient);
                await LoadIngredientsAsync();

                if (SelectedIngredient?.Id == updatedIngredient.Id)
                {
                    SelectedIngredient = updatedIngredient;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Error updating ingredient: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task DeleteIngredient(Ingredient ingredient)
    {
        var dialog = new ContentDialog
        {
            Title = "Confirm Deletion",
            Content = $"Are you sure you want to delete {ingredient.Name}? This will also remove all transaction history for this ingredient.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await _contentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                // First, delete all transactions related to this ingredient
                var allTransactions = await _dao.IngredientInventoryTransactions.GetAll();
                foreach (var transaction in allTransactions.Where(t => t.IngredientId == ingredient.Id))
                {
                    await _dao.IngredientInventoryTransactions.Delete(transaction.Id);
                }

                // Then delete the ingredient
                await _dao.Ingredients.Delete(ingredient.Id);

                if (SelectedIngredient?.Id == ingredient.Id)
                {
                    SelectedIngredient = null;
                    Transactions.Clear();
                }

                await LoadIngredientsAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Error deleting ingredient: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task AddTransaction(Ingredient ingredient)
    {
        var contentPanel = new StackPanel { Spacing = 10 };

        var transactionTypeCombo = new ComboBox
        {
            Width = 200,
            ItemsSource = new List<string> { "IMPORT", "EXPORT" },
            SelectedIndex = 0
        };

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Type:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                transactionTypeCombo
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Quantity:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new NumberBox { Name = "QuantityBox", Value = 1, Minimum = 1, Width = 200 }
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Unit Price:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new NumberBox { Name = "PriceBox", Value = 0, Minimum = 0, Width = 200 }
            }
        });

        var dialog = new ContentDialog
        {
            Title = $"Add Transaction for {ingredient.Name}",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = contentPanel,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await _contentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                var transactionType = transactionTypeCombo.SelectedItem.ToString();
                var quantityBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<NumberBox>().FirstOrDefault()?.Name == "QuantityBox")?.Children.OfType<NumberBox>().FirstOrDefault();
                var priceBox = contentPanel.Children.OfType<StackPanel>().FirstOrDefault(sp => sp.Children.OfType<NumberBox>().FirstOrDefault()?.Name == "PriceBox")?.Children.OfType<NumberBox>().FirstOrDefault();

                var quantity = (int)(quantityBox?.Value ?? 0);

                if (quantity <= 0)
                {
                    await ShowErrorMessage("Quantity must be greater than zero");
                    return;
                }

                // Check if export would reduce inventory below 0
                if (transactionType == "EXPORT" && quantity > ingredient.Quantity)
                {
                    await ShowErrorMessage($"Cannot export {quantity} {ingredient.Unit}(s). Only {ingredient.Quantity} available in inventory.");
                    return;
                }

                // Create the transaction record
                var transaction = new IngredientInventoryTransaction
                {
                    IngredientId = ingredient.Id,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Quantity = quantity,
                    Unit = ingredient.Unit,
                    TransactionType = transactionType,
                    UnitPrice = priceBox?.Value ?? 0
                };

                await _dao.IngredientInventoryTransactions.Add(transaction);

                // Update the ingredient quantity
                var updatedQuantity = transactionType == "IMPORT"
                    ? ingredient.Quantity + quantity
                    : ingredient.Quantity - quantity;

                var updatedIngredient = new Ingredient
                {
                    Id = ingredient.Id,
                    Name = ingredient.Name,
                    Unit = ingredient.Unit,
                    Quantity = updatedQuantity,
                    Threshold = ingredient.Threshold
                };

                await _dao.Ingredients.Update(updatedIngredient);

                // Refresh data
                await LoadIngredientsAsync();

                if (SelectedIngredient?.Id == ingredient.Id)
                {
                    await LoadTransactionsAsync(updatedIngredient);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Error adding transaction: {ex.Message}");
            }
        }
    }

    private async Task ShowErrorMessage(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
}