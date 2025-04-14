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

    [ObservableProperty]
    private string productCategory = string.Empty;

    public ProductsDetailViewModel(IDao dao, INavigationService navigationService)
    {
        _dao = dao;
        _navigationService = navigationService;
    }

    public async void OnNavigatedTo(object parameter)
    {
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
        Item = null;
        Item = await _dao.Products.GetById(productId);
        ProductCategory = (await _dao.Categories.GetById(Item.CategoryId)).Name;

        RelateProducts.Clear();
        (await _dao.Products.GetAll()).ToList().ForEach(p =>
        {
            if (p.Id != Item.Id && p.CategoryId == Item.CategoryId)
                RelateProducts.Add(p);
        });
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
            var priceBox = (TextBox)((StackPanel)contentPanel.Children[1]).Children[1];
            var descriptionBox = (TextBox)((StackPanel)contentPanel.Children[2]).Children[1];
            var categoryBox = (ComboBox)((StackPanel)contentPanel.Children[3]).Children[1];
            var inStockBox = (CheckBox)((StackPanel)contentPanel.Children[4]).Children[1];

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
}
