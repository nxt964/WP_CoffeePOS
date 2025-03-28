using CoffeePOS.Contracts.Services;
using CoffeePOS.ViewModels;
using CoffeePOS.Views;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<DashboardViewModel, DashboardPage>();
        Configure<CategoriesViewModel, CategoriesPage>();
        Configure<ProductsViewModel, ProductsPage>();
        Configure<ProductsDetailViewModel, ProductsDetailPage>();
        Configure<CustomersViewModel, CustomersPage>();
        Configure<InventoryViewModel, InventoryPage>();
        Configure<MaterialViewModel, MaterialPage>();
        Configure<TableViewModel, TablePage>();
        Configure<TableDetailViewModel, TableDetailPage>();
        Configure<StatisticsViewModel, StatisticsPage>();
        Configure<WebViewViewModel, WebViewPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<ProductViewModel, ProductPage>();
        Configure<AddProductViewModel, AddProductPage>();
        Configure<UpdateProductViewModel, UpdateProductPage>();
        Configure<OrderViewModel, OrderPage>();
        Configure<AddOrderViewModel, AddOrderPage>();
        Configure<DetailOrderViewModel, DetailOrderPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
