using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePOS.ViewModels;

public partial class AddOrderViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private Order order = new Order();

    public AddOrderViewModel()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        order = new Order();
    }

    public void OnNavigatedFrom()
    {
    }
}