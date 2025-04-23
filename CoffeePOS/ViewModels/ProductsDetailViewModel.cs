using System.Collections.ObjectModel;
using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Contracts.Services;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using CoffeePOS.Contracts.Services;
using System.Diagnostics;
using CoffeePOS.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CoffeePOS.ViewModels;

public partial class ProductsDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;
    private readonly INavigationService _navigationService;
    private ContentDialogHelper ContentDialogHelper { get; } = new ContentDialogHelper();

    [ObservableProperty]
    private Product? item;

    public ObservableCollection<Product> RelateProducts { get; } = new();

    // New collection for product ingredients
    public ObservableCollection<ProductIngredientDetail> ProductIngredients { get; } = new();

    [ObservableProperty]
    private string productCategory = string.Empty;

    [ObservableProperty]
    private bool isLoadingIngredients = false;

    public ProductsDetailViewModel(IDao dao, INavigationService navigationService)
    {
        _dao = dao;
        _navigationService = navigationService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Debug.WriteLine($"[DEBUG] ProductsDetailViewModel.OnNavigatedTo: {parameter}");
        int productId;

        if (parameter is string str && int.TryParse(str.Split('-')[0], out int parsedId))
        {
            productId = parsedId;
        }
        else if (parameter is int id)
        {
            productId = id;
        }
        else return;

        await LoadData(productId);
    }

    private async Task LoadData(int productId)
    {
        Debug.WriteLine($"[DEBUG] ProductsDetailViewModel.LoadData: Loading product {productId}");
        Item = null;
        Item = await _dao.Products.GetById(productId);

        if (Item == null)
        {
            Debug.WriteLine($"[DEBUG] ProductsDetailViewModel.LoadData: Product {productId} not found");
            return;
        }

        ProductCategory = (await _dao.Categories.GetById(Item.CategoryId)).Name;

        // Load related products
        RelateProducts.Clear();
        (await _dao.Products.GetAll()).ToList().ForEach(p =>
        {
            if (p.Id != Item.Id && p.CategoryId == Item.CategoryId)
                RelateProducts.Add(p);
        });

        // Load ingredients
        await LoadProductIngredients(productId);
    }

    private async Task LoadProductIngredients(int productId)
    {
        IsLoadingIngredients = true;

        try
        {
            ProductIngredients.Clear();

            // Get all product ingredients for this product
            var allProductIngredients = await _dao.ProductIngredients.GetAll();
            var productIngredients = allProductIngredients.Where(pi => pi.ProductId == productId).ToList();

            if (productIngredients.Any())
            {
                // Get all ingredients
                var allIngredients = await _dao.Ingredients.GetAll();

                // Create detailed view models
                foreach (var pi in productIngredients)
                {
                    var ingredient = allIngredients.FirstOrDefault(i => i.Id == pi.IngredientId);
                    if (ingredient != null)
                    {
                        ProductIngredients.Add(new ProductIngredientDetail
                        {
                            Ingredient = ingredient,
                            QuantityUsed = pi.QuantityUsed
                        });
                    }
                }
            }

            foreach(var ingredient in ProductIngredients)
            {
                Debug.WriteLine($"[DEBUG] IngredientId {ingredient?.Ingredient?.Id}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] ProductsDetailViewModel.LoadProductIngredients: {ex.Message}");
        }
        finally
        {
            IsLoadingIngredients = false;
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnProductClicked(int productId)
    {
        _navigationService.NavigateTo(typeof(ProductsDetailViewModel).FullName, productId);
    }

    [RelayCommand]
    public async void EditProduct()
    {
        var dialog = ContentDialogHelper.CreateProductDialog(item, false);

        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var contentPanel = (StackPanel)((ContentDialog)dialog).Content;

            var nameBox = (TextBox)((StackPanel)contentPanel.Children[0]).Children[1];
            var priceBox = (TextBox)((StackPanel)contentPanel.Children[2]).Children[1];
            var descriptionBox = (TextBox)((StackPanel)contentPanel.Children[4]).Children[1];
            var categoryBox = (ComboBox)((StackPanel)contentPanel.Children[6]).Children[1];
            var inStockBox = (CheckBox)((StackPanel)contentPanel.Children[8]).Children[1];

            var updatedItem = new Product
            {
                Id = item.Id,
                Name = nameBox.Text,
                Price = double.TryParse(priceBox.Text, out double price) ? price : 0,
                Image = item.Image,
                IsStocked = inStockBox.IsChecked ?? false,
                Description = descriptionBox.Text,
                CategoryId = (await _dao.Categories.GetAll())
                .FirstOrDefault(c => c.Name == categoryBox.SelectedItem.ToString())?.Id ?? 0
            };

            await _dao.Products.Update(updatedItem);

            await LoadData(updatedItem.Id);
        }
    }

    [RelayCommand]
    public async void DeleteProduct()
    {
        var contentPanel = new StackPanel();

        contentPanel.Children.Add(
            new TextBlock
            {
                Text = $"Are you sure you want to delete product {item.Name}?",
                Margin = new Thickness(0, 0, 0, 15)
            }
        );

        contentPanel.Children.Add(
            new Image
            {
                Width = 150,
                Height = 150,
                Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill,
                Source = new BitmapImage(new Uri(
                    !string.IsNullOrEmpty(item.Image)
                    ? item.Image
                    : "ms-appx:///Assets/ProductImageDefault.png"))
            }
        );

        var dialog = new ContentDialog
        {
            Title = "Delete Product",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            Content = contentPanel,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            await _dao.Products.Delete(item.Id);
            _navigationService.GoBack();
        }
    }

    [RelayCommand]
    public async Task AddIngredient()
    {
        if (Item == null) return;

        var ingredients = await _dao.Ingredients.GetAll();

        // Filter out ingredients that are already added
        var availableIngredients = ingredients.Where(i =>
            !ProductIngredients.Any(pi => pi.Ingredient.Id == i.Id)).ToList();

        if (!availableIngredients.Any())
        {
            var errorDialog = new ContentDialog
            {
                Title = "No Available Ingredients",
                Content = "All ingredients have already been added to this product.",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            await ContentDialogHelper.ShowDialogWithSlideIn(errorDialog);
            return;
        }

        var contentPanel = new StackPanel { Spacing = 10 };

        var ingredientCombo = new ComboBox
        {
            Width = 200,
            PlaceholderText = "Select ingredient",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        foreach (var ingredient in availableIngredients)
        {
            ingredientCombo.Items.Add(ingredient.Name);
        }

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                new TextBlock { Text = "Ingredient:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                ingredientCombo
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Quantity:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
                new NumberBox
                {
                    Name = "QuantityBox",
                    PlaceholderText = "Amount needed",
                    Minimum = 0.1,
                    Value = 1,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                    Width = 200
                }
            }
        });

        var dialog = new ContentDialog
        {
            Title = "Add Ingredient",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = contentPanel,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                if (ingredientCombo.SelectedIndex < 0)
                {
                    return;
                }

                var selectedIngredientName = ingredientCombo.SelectedItem.ToString();
                var selectedIngredient = availableIngredients.FirstOrDefault(i => i.Name == selectedIngredientName);

                if (selectedIngredient == null)
                {
                    return;
                }

                var quantityBox = contentPanel.Children.OfType<StackPanel>()
                    .ElementAt(1)
                    .Children.OfType<NumberBox>()
                    .FirstOrDefault();

                var quantity = (decimal)(quantityBox?.Value ?? 1);

                if (quantity <= 0)
                {
                    return;
                }

                // Create a new product ingredient
                var productIngredient = new ProductIngredient
                {
                    ProductId = Item.Id,
                    IngredientId = selectedIngredient.Id,
                    QuantityUsed = quantity
                };

                await _dao.ProductIngredients.Add(productIngredient);

                // Refresh the ingredients
                await LoadProductIngredients(Item.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ProductsDetailViewModel.AddIngredient: {ex.Message}");

                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while adding the ingredient.",
                    CloseButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                await ContentDialogHelper.ShowDialogWithSlideIn(errorDialog);
            }
        }
    }

    [RelayCommand]
    public async Task RemoveIngredient(ProductIngredientDetail ingredientDetail)
    {
        if (Item == null || ingredientDetail == null) return;

        var confirmDialog = new ContentDialog
        {
            Title = "Remove Ingredient",
            Content = $"Are you sure you want to remove {ingredientDetail.Ingredient.Name} from this product?",
            PrimaryButtonText = "Remove",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await ContentDialogHelper.ShowDialogWithSlideIn(confirmDialog);

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                // Find the product ingredient
                var allProductIngredients = await _dao.ProductIngredients.GetAll();
                var productIngredient = allProductIngredients.FirstOrDefault(
                    pi => pi.ProductId == Item.Id && pi.IngredientId == ingredientDetail.Ingredient.Id);

                if (productIngredient != null)
                {
                    await _dao.ProductIngredients.Delete(productIngredient.Id);

                    // Refresh the ingredients
                    await LoadProductIngredients(Item.Id);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ProductsDetailViewModel.RemoveIngredient: {ex.Message}");

                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while removing the ingredient.",
                    CloseButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                await ContentDialogHelper.ShowDialogWithSlideIn(errorDialog);
            }
        }
    }
}

// Helper class for binding product ingredients
public class ProductIngredientDetail
{
    public Ingredient Ingredient
    {
        get; set;
    }
    public decimal QuantityUsed
    {
        get; set;
    }
}