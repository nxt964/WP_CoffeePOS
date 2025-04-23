using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeePOS.ViewModels;

public partial class AddProductToOrderDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;
    private int _orderId;
    private bool _isDialogShowing;

    [ObservableProperty]
    private ObservableCollection<CoffeePOS.Models.Product> products = new ObservableCollection<CoffeePOS.Models.Product>();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private string noProductsSelectedError;

    [ObservableProperty]
    private bool canAddProducts = false;

    public AddProductToOrderDetailViewModel(IDao dao)
    {
        _dao = dao;
        Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel Constructor: _dao is null: {_dao == null}");
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is (int orderId, IDao dao))
        {
            _orderId = orderId;
            Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.OnNavigatedTo: Navigating to OrderId = {orderId}");
            LoadProductsCommand.Execute(null);
        }
    }

    public void OnNavigatedFrom()
    {
        Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.OnNavigatedFrom: Leaving AddProductToOrderDetailPage...");
    }

    [RelayCommand]
    private async Task LoadProducts()
    {
        try
        {
            Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.LoadProducts: Loading products...");
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
            Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.LoadProducts: Loaded {Products.Count} products");
            UpdateCanAddProducts(); 
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.LoadProducts: {ex.Message}");
            Products.Clear();
            UpdateCanAddProducts();
        }
    }

    [RelayCommand]
    private async Task SearchProducts()
    {
        try
        {
            Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.SearchProducts: Searching for query '{SearchQuery}'");
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
            Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.SearchProducts: Found {Products.Count} products");
            UpdateCanAddProducts();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.SearchProducts: {ex.Message}");
            Products.Clear();
            UpdateCanAddProducts();
        }
    }

    [RelayCommand]
    private async Task AddProducts(Frame frame)
    {
        try
        {
            NoProductsSelectedError = null;
            Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Reset error messages");

            var selectedProducts = Products.Where(p => p.IsSelected).ToList();
            if (!selectedProducts.Any())
            {
                Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.AddProducts: No products selected");
                NoProductsSelectedError = "Please select at least one product to add to the order.";
                Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Set NoProductsSelectedError = {NoProductsSelectedError}");
                return;
            }

            Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Adding {selectedProducts.Count} products to OrderId = {_orderId}");

            var existingOrderDetails = await _dao.OrderDetails.GetAll();
            var orderDetailsForThisOrder = existingOrderDetails.Where(od => od.OrderId == _orderId).ToList();

            foreach (var product in selectedProducts)
            {
                var existingOrderDetail = orderDetailsForThisOrder.FirstOrDefault(od => od.ProductId == product.Id);
                if (existingOrderDetail != null)
                {
                    existingOrderDetail.Quantity += 1;
                    await _dao.OrderDetails.Update(existingOrderDetail);
                    Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Increased quantity for ProductId = {product.Id}, new Quantity = {existingOrderDetail.Quantity}");
                }
                else
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = _orderId,
                        ProductId = product.Id,
                        Price = (decimal)product.Price,
                        Quantity = 1
                    };
                    await _dao.OrderDetails.Add(orderDetail);
                    Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Added new OrderDetail for ProductId = {product.Id}, Quantity = 1");
                }
            }

            await _dao.SaveChangesAsync();
            Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.AddProducts: Products added/updated successfully");

            await ShowSuccessDialog("Success", "Products added to the order successfully.", frame.XamlRoot);

            frame.GoBack();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.AddProducts: {ex.Message}");
            await ShowErrorDialog("Error", $"Failed to add products to the order: {ex.Message}", frame.XamlRoot);
        }
    }

    [RelayCommand]
    private void Back(Frame frame)
    {
        Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.Back: Navigating back");
        frame.GoBack();
    }

    partial void OnProductsChanged(ObservableCollection<CoffeePOS.Models.Product> value)
    {
        if (value.Any(p => p.IsSelected))
        {
            if (NoProductsSelectedError != null)
            {
                Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.OnProductsChanged: Clearing NoProductsSelectedError = {NoProductsSelectedError}");
                NoProductsSelectedError = null;
            }
        }
        UpdateCanAddProducts();
    }

    private void UpdateCanAddProducts()
    {
        CanAddProducts = Products.Any(p => p.IsSelected);
        Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.UpdateCanAddProducts: CanAddProducts = {CanAddProducts}");
    }

    private async Task ShowErrorDialog(string title, string content, XamlRoot xamlRoot)
    {
        if (_isDialogShowing)
        {
            Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.ShowErrorDialog: Another dialog is already showing. Skipping...");
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        var enqueueResult = DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
        {
            try
            {
                if (xamlRoot == null)
                {
                    Debug.WriteLine("[ERROR] AddProductToOrderDetailViewModel.ShowErrorDialog: xamlRoot is null, cannot show dialog");
                    tcs.SetResult(false);
                    return;
                }

                _isDialogShowing = true;
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = "OK",
                    XamlRoot = xamlRoot
                };
                await dialog.ShowAsync();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.ShowErrorDialog: Failed to show dialog. Exception: {ex.Message}");
                tcs.SetResult(false);
            }
            finally
            {
                _isDialogShowing = false;
            }
        });

        Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.ShowErrorDialog: DispatcherQueue.TryEnqueue result: {enqueueResult}");
        if (!enqueueResult)
        {
            Debug.WriteLine("[ERROR] AddProductToOrderDetailViewModel.ShowErrorDialog: Failed to enqueue dialog creation on UI thread");
            tcs.SetResult(false);
        }

        await tcs.Task;
    }

    private async Task ShowSuccessDialog(string title, string content, XamlRoot xamlRoot)
    {
        if (_isDialogShowing)
        {
            Debug.WriteLine("[DEBUG] AddProductToOrderDetailViewModel.ShowSuccessDialog: Another dialog is already showing. Skipping...");
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        var enqueueResult = DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
        {
            try
            {
                if (xamlRoot == null)
                {
                    Debug.WriteLine("[ERROR] AddProductToOrderDetailViewModel.ShowSuccessDialog: xamlRoot is null, cannot show dialog");
                    tcs.SetResult(false);
                    return;
                }

                _isDialogShowing = true;
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = "OK",
                    XamlRoot = xamlRoot
                };
                await dialog.ShowAsync();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] AddProductToOrderDetailViewModel.ShowSuccessDialog: Failed to show dialog. Exception: {ex.Message}");
                tcs.SetResult(false);
            }
            finally
            {
                _isDialogShowing = false;
            }
        });

        Debug.WriteLine($"[DEBUG] AddProductToOrderDetailViewModel.ShowSuccessDialog: DispatcherQueue.TryEnqueue result: {enqueueResult}");
        if (!enqueueResult)
        {
            Debug.WriteLine("[ERROR] AddProductToOrderDetailViewModel.ShowSuccessDialog: Failed to enqueue dialog creation on UI thread");
            tcs.SetResult(false);
        }

        await tcs.Task;
    }
}