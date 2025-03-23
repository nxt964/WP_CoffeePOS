using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CoffeePOS.ViewModels;

public partial class ProductViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private ObservableCollection<Product> source = new ObservableCollection<Product>();

    public ProductViewModel()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        source.Clear();
        // Giả lập dữ liệu sản phẩm
        source.Add(new Product
        {
            Id = "P001",
            Name = "Sample Product 1",
            Price = 100.0,
            Image = "/Assets/StoreLogo.png"
        });
        source.Add(new Product
        {
            Id = "P002",
            Name = "Sample Product 2",
            Price = 200.0,
            Image = "/Assets/StoreLogo.png"
        });
        source.Add(new Product
        {
            Id = "P003",
            Name = "Sample Product 3",
            Price = 300.0,
            Image = "/Assets/StoreLogo.png"
        });
    }

    public void OnNavigatedFrom()
    {
    }

    public void DeleteProduct(string productId)
    {
        var productToRemove = source.FirstOrDefault(p => p.Id == productId);
        if (productToRemove != null)
        {
            source.Remove(productToRemove);
        }
    }
}