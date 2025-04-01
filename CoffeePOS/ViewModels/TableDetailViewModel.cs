using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace CoffeePOS.ViewModels;

public partial class TableDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;

    [ObservableProperty]
    private Table? _table;

    [ObservableProperty]
    private ObservableCollection<Reservation> _reservations = new();

    public TableDetailViewModel(IDao dao)
    {
        _dao = dao;
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is int tableId)
        {
            Table = await _dao.Tables.GetById(tableId);
            await LoadReservations();
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task LoadReservations()
    {
        if (Table == null) return;

        Reservations.Clear();
        var allReservations = await _dao.Reservations.GetAll();
        var tableReservations = allReservations.Where(r => r.TableId == Table.Id).ToList();

        foreach (var reservation in tableReservations)
        {
            Reservations.Add(reservation);
        }
    }

    [RelayCommand]
    private async Task ChangeStatus(string newStatus)
    {
        if (Table == null) return;

        Table.Status = newStatus;
        await _dao.Tables.Update(Table);
    }
}