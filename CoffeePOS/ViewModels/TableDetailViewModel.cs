using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Contracts.Services;
using CoffeePOS.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePOS.ViewModels;

public partial class TableDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    [ObservableProperty]
    private SampleOrder? item;

    public TableDetailViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is long orderID)
        {
            var data = await _sampleDataService.GetContentGridDataAsync();
            Item = data.First(i => i.OrderID == orderID);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
