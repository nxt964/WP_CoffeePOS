
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePOS.Models;

public partial class OrderDetailDisplay : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string productName;

    [ObservableProperty]
    private int quantity;

    [ObservableProperty]
    private decimal price;

    [ObservableProperty]
    private string image;

    [ObservableProperty]
    private bool isEditable;

    [RelayCommand]
    private void IncrementQuantity()
    {
        if (IsEditable)
        {
            Quantity++;
        }
    }

    [RelayCommand]
    private void DecrementQuantity()
    {
        if (IsEditable && Quantity > 1)
        {
            Quantity--;
        }
    }
}