using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeePOS.Models;

public class OrderDetailDisplay : INotifyPropertyChanged
{
    private int _quantity;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Id
    {
        get; set;
    }
    public int ProductId
    {
        get; set;
    }
    public string ProductName
    {
        get; set;
    }
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
    }
    public decimal Price
    {
        get; set;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}