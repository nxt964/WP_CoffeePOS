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
        Source.Clear();
        // Giả lập dữ liệu sản phẩm
        Source.Add(new Product
        {
            Id = "P001",
            Name = "Sample Product 1",
            Price = 100.0,
            Image = "/Assets/StoreLogo.png"
        });
        Source.Add(new Product
        {
            Id = "P002",
            Name = "Sample Product 2",
            Price = 200.0,
            Image = "/Assets/StoreLogo.png"
        });
        Source.Add(new Product
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
        var productToRemove = Source.FirstOrDefault(p => p.Id == productId);
        if (productToRemove != null)
        {
            Source.Remove(productToRemove);
        }
    }
}