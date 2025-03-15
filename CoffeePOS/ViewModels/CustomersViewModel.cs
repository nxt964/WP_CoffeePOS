using System.Collections.ObjectModel;

using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Contracts.Services;
using CoffeePOS.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePOS.ViewModels;

public partial class CustomersViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

    public CustomersViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetGridDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
