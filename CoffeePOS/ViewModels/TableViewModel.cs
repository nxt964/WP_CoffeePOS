using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

using CoffeePOS.Contracts.Services;
using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Contracts.Services;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.ViewModels;

public partial class TableViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IDao _dao;

    private ContentDialogHelper ContentDialogHelper { get; } = new ContentDialogHelper();

    private List<Table> _allTables = new();
    public ObservableCollection<Table> Tables { get; } = new ObservableCollection<Table>();

    [ObservableProperty]
    private Table _selectedTable;

    public TableViewModel(INavigationService navigationService, IDao dao)
    {
        _navigationService = navigationService;
        _dao = dao;
    }

    private async Task LoadData()
    {
        await LoadTables();

        StatusSelection.Clear();
        StatusSelection.Add("All");
        StatusSelection.Add("Available");
        StatusSelection.Add("Occupied");
        StatusSelection.Add("Reserved");
        StatusSelection.Add("Maintenance");

        SearchNumber = "";
        Status = "All";
    }

    private async Task LoadTables()
    {
        _allTables.Clear();
        (await _dao.Tables.GetAll()).ToList().ForEach(_allTables.Add);
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadData();
        ApplyFilters();
    }

    public void OnNavigatedFrom()
    {
    }

    private string _searchNumber = "";
    public string SearchNumber
    {
        get => _searchNumber;
        set
        {
            SetProperty(ref _searchNumber, value);
            OnPropertyChanged(nameof(IsClearButtonVisible));
        }
    }

    public bool IsClearButtonVisible => !string.IsNullOrEmpty(SearchNumber);

    [RelayCommand]
    private void ClearSearch()
    {
        SearchNumber = "";
        OnPropertyChanged(nameof(IsClearButtonVisible));
        ApplyFilters();
    }

    private string _status = "All";
    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    private ObservableCollection<string> _statusSelection = new();
    public ObservableCollection<string> StatusSelection
    {
        get => _statusSelection;
        set
        {
            _statusSelection = value;
            OnPropertyChanged(nameof(StatusSelection));
        }
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var filtered = _allTables.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchNumber))
        {
            filtered = filtered.Where(t => t.TableNumber.Contains(SearchNumber, StringComparison.OrdinalIgnoreCase));
        }

        if (Status != "All" && !string.IsNullOrEmpty(Status))
        {
            filtered = filtered.Where(t => t.Status.Equals(Status, StringComparison.OrdinalIgnoreCase));
        }

        _filteredTables = filtered.ToList();
        _currentPage = 1;
        UpdatePagedTables();
    }

    [RelayCommand]
    private void RemoveFilter()
    {
        SearchNumber = "";
        Status = "All";
        ApplyFilters();
    }

    [RelayCommand]
    private async void AddTable()
    {
        Debug.WriteLine("AddTable");
        var table = new Table { Status = "Available" };
        var dialog = ContentDialogHelper.CreateTableDialog(table, true);

        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var contentPanel = (StackPanel)dialog.Content;

            // Lấy TextBox TableNumber (phần tử thứ 2 trong StackPanel con đầu tiên)
            var tableNumberBox = (TextBox)((StackPanel)contentPanel.Children[0]).Children[1];

            // Lấy ComboBox Status (phần tử thứ 2 trong StackPanel con thứ hai)
            var statusCombo = (ComboBox)((StackPanel)contentPanel.Children[1]).Children[1];

            if (string.IsNullOrWhiteSpace(tableNumberBox.Text))
            {
                var errorDialog = ContentDialogHelper.CreateErrorDialog("Table number cannot be empty.");
                await ContentDialogHelper.ShowDialogWithSlideIn(errorDialog);
                return;
            }

            table.TableNumber = tableNumberBox.Text;
            table.Status = statusCombo.SelectedItem.ToString();

            await _dao.Tables.Add(table);
            await LoadTables();
            ApplyFilters();
        }
    }

    

    [RelayCommand]
    private async void EditTable(Table table)
    {

        Debug.WriteLine("EditTable");

        if (table == null) return;

        var dialog = ContentDialogHelper.CreateTableDialog(table, false);
        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var contentPanel = (StackPanel)dialog.Content;

            // Lấy TextBox TableNumber (phần tử thứ 2 trong StackPanel con đầu tiên)
            var tableNumberBox = (TextBox)((StackPanel)contentPanel.Children[0]).Children[1];

            // Lấy ComboBox Status (phần tử thứ 2 trong StackPanel con thứ hai)
            var statusCombo = (ComboBox)((StackPanel)contentPanel.Children[1]).Children[1];

            if (string.IsNullOrWhiteSpace(tableNumberBox.Text))
            {
                var errorDialog = ContentDialogHelper.CreateErrorDialog("Table number cannot be empty.");
                await ContentDialogHelper.ShowDialogWithSlideIn(errorDialog);
                return;
            }

            table.TableNumber = tableNumberBox.Text;
            table.Status = statusCombo.SelectedItem.ToString();

            await _dao.Tables.Update(table);
            await LoadTables();
            ApplyFilters();
        }
    }

    [RelayCommand]
    private async void DeleteTable(Table table)
    {
        if (table == null) return;

        var dialog = ContentDialogHelper.CreateDeleteTableConfirmationDialog(table);
        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            await _dao.Tables.Delete(table.Id);
            await LoadTables();
            ApplyFilters();
        }
    }

    [RelayCommand]
    private async void ChangeStatus(Table table)
    {
        if (table == null) return;

        var dialog = ContentDialogHelper.CreateTableStatusDialog(table);
        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var contentPanel = (StackPanel)dialog.Content;

            // Status ComboBox là phần tử thứ 2 trong StackPanel con thứ hai
            var statusCombo = (ComboBox)((StackPanel)contentPanel.Children[1]).Children[1];

            table.Status = statusCombo.SelectedItem.ToString();

            await _dao.Tables.Update(table);
            await LoadTables();
            ApplyFilters();
        }
    }

    // Pagination
    private int _currentPage = 1;
    private int _itemsPerPage = 10;
    private List<Table> _filteredTables = new();

    public int ItemsPerPage
    {
        get => _itemsPerPage;
        set
        {
            _currentPage = 1;
            SetProperty(ref _itemsPerPage, value);
            UpdatePagedTables();
        }
    }

    private void UpdatePagedTables()
    {
        Tables.Clear();
        var pagedTables = _filteredTables.Skip((_currentPage - 1) * _itemsPerPage).Take(_itemsPerPage).ToList();
        pagedTables.ForEach(Tables.Add);
        OnPropertyChanged(nameof(PageIndicator));
    }

    public string PageIndicator => $"{_currentPage}/{Math.Max(1, (int)Math.Ceiling((double)_filteredTables.Count / _itemsPerPage))}";

    [RelayCommand]
    private void NextPage()
    {
        if (_currentPage < (int)Math.Ceiling((double)_filteredTables.Count / _itemsPerPage))
        {
            _currentPage++;
            UpdatePagedTables();
            OnPropertyChanged(nameof(PageIndicator));
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            UpdatePagedTables();
            OnPropertyChanged(nameof(PageIndicator));
        }
    }
}
