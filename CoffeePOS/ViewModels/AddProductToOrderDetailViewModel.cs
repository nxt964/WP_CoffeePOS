using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeePOS.ViewModels;

public partial class AddProductToOrderDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;

    [ObservableProperty]
    private ObservableCollection<CoffeePOS.Models.Product> products = new ObservableCollection<CoffeePOS.Models.Product>();

    [ObservableProperty]
    private string searchQuery;

    private int _orderId;

    public AddProductToOrderDetailViewModel(IDao dao)
    {
        _dao = dao;
        System.Diagnostics.Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel Constructor: _dao is null: {_dao == null}");
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is (int orderId, IDao dao))
        {
            _orderId = orderId;
            LoadProductsCommand.Execute(null);
        }
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.OnNavigatedFrom: Leaving AddProductToOrderDetailPage...");
    }

    [RelayCommand]
    private async Task LoadProducts()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.LoadProducts: Loading products...");
            Products.Clear();
            var backendProducts = await _dao.Products.GetAll();
            foreach (var backendProduct in backendProducts)
            {
                Products.Add(new CoffeePOS.Models.Product
                {
                    Id = backendProduct.Id,
                    Name = backendProduct.Name,
                    Price = backendProduct.Price,
                    IsStocked = backendProduct.IsStocked,
                    CategoryId = backendProduct.CategoryId,
                    IsSelected = false
                });
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.LoadProducts: Loaded {Products.Count} products");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.LoadProducts: {ex.Message}");
            Products.Clear();
        }
    }

    [RelayCommand]
    private async Task SearchProducts()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.SearchProducts: Searching for query '{SearchQuery}'");
            var selectedIds = Products.Where(p => p.IsSelected).Select(p => p.Id).ToList();

            var filtered = (await _dao.Products.GetAll())
                .Where(p => string.IsNullOrWhiteSpace(SearchQuery) || p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Products.Clear();
            foreach (var backendProduct in filtered)
            {
                Products.Add(new CoffeePOS.Models.Product
                {
                    Id = backendProduct.Id,
                    Name = backendProduct.Name,
                    Price = backendProduct.Price,
                    IsStocked = backendProduct.IsStocked,
                    CategoryId = backendProduct.CategoryId,
                    IsSelected = selectedIds.Contains(backendProduct.Id)
                });
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.SearchProducts: Found {Products.Count} products");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.SearchProducts: {ex.Message}");
            Products.Clear();
        }
    }

    [RelayCommand]
    private async Task AddProducts(Frame frame)
    {
        try
        {
            var selectedProducts = Products.Where(p => p.IsSelected).ToList();
            if (!selectedProducts.Any())
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.AddProducts: No products selected");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please select at least one product to add to the order.",
                    CloseButtonText = "OK",
                    XamlRoot = frame.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Adding {selectedProducts.Count} products to OrderId = {_orderId}");
            foreach (var product in selectedProducts)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = _orderId,
                    ProductId = product.Id,
                    Price = (decimal)product.Price,
                    Quantity = 1
                };
                await _dao.OrderDetails.Add(orderDetail);
            }

            await _dao.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Products added successfully");

            var successDialog = new ContentDialog
            {
                Title = "Success",
                Content = "Products added to the order successfully.",
                CloseButtonText = "OK",
                XamlRoot = frame.XamlRoot
            };
            await successDialog.ShowAsync();
            frame.GoBack();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.AddProducts: {ex.Message}");
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to add products to the order: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = frame.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    [RelayCommand]
    private void Back(Frame frame)
    {
        System.Diagnostics.Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.Back: Navigating back");
        frame.GoBack();
    }
}