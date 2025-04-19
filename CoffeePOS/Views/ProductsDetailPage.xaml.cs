using System.Diagnostics;
using CoffeePOS.Contracts.Services;
using CoffeePOS.Core.Models;
using CoffeePOS.ViewModels;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoffeePOS.Views;

public sealed partial class ProductsDetailPage : Page
{
    public ProductsDetailViewModel ViewModel
    {
        get;
    }

    public ProductsDetailPage()
    {
        ViewModel = App.GetService<ProductsDetailViewModel>();
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.OnNavigatedTo(e.Parameter);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {
            var navigationService = App.GetService<INavigationService>();
            if (ViewModel.Item != null)
            {
                navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }

    private void OnProductClicked(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] ProductsDetailPage.OnProductClicked: Clicked on related product");
        if (sender is Button button && button.Tag is int productId)
        {
            ViewModel.OnProductClicked(productId);
        }
    }

    private void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] ProductsDetailPage.EditProduct_Click: Editing product");
        ViewModel.EditProductCommand.Execute(null);
    }

    private void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] ProductsDetailPage.DeleteProduct_Click: Deleting product");
        ViewModel.DeleteProductCommand.Execute(null);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] ProductsDetailPage.BackButton_Click: Navigating back");
        var navigationService = App.GetService<INavigationService>();
        navigationService.GoBack();
    }

    private void AddIngredient_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] ProductsDetailPage.AddIngredient_Click: Adding ingredient");
        ViewModel.AddIngredientCommand.Execute(null);
    }

    private void RemoveIngredient_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] ProductsDetailPage.RemoveIngredient_Click: Removing ingredient");
        if (sender is Button button && button.Tag is ProductIngredientDetail ingredientDetail)
        {
            ViewModel.RemoveIngredientCommand.Execute(ingredientDetail);
        }
    }
}