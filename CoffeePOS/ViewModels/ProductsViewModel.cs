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

public partial class ProductsViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IDao _dao;

    private ContentDialogHelper ContentDiaglogHelper { get; } = new ContentDialogHelper();

    private List<Product> _allProducts = new();
    public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

    public ProductsViewModel(INavigationService navigationService, IDao dao)
    {
        _navigationService = navigationService; 
        _dao = dao;
    }

    private async Task LoadData()
    {
        await LoadProducts();

        CategorySelection.Clear();
        CategorySelection.Add("All");
        CategoryMap.Clear();
        (await _dao.Categories.GetAll()).ToList().ForEach((c) => {
            CategorySelection.Add(c.Name);
            CategoryMap[c.Id] = c.Name;
        });

        PriceSelection = new ObservableCollection<string>()
        {
            "All",
            "Under $2.00",
            "$2.00 - $3.99",
            "$4.00 - $5.99",
            "$6.00 - $8.00",
            "Over $8",
        };

        SortSelection = new ObservableCollection<string>()
        {
            "All",
            "Price ascending",
            "Price descending",
        };

        SearchName = Price = Category = Sort = string.Empty;

        
    }

    private async Task LoadProducts()
    {
        _allProducts.Clear();
        (await _dao.Products.GetAll()).ToList().ForEach(_allProducts.Add);
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadData();
        ApplyFilters();
    }



    public void OnNavigatedFrom()
    {
    }


    private string _searchName = "";
    public string SearchName
    {
        get => _searchName;
        set
        {
            SetProperty(ref _searchName, value);
            OnPropertyChanged(nameof(SearchName));
        }
    }

    public bool IsClearButtonVisible => !string.IsNullOrEmpty(SearchName);

    [RelayCommand]
    private void ClearSearch()
    {
        SearchName = "";
        OnPropertyChanged(nameof(IsClearButtonVisible));
        ApplyFilters();
    }

    private string _price = "";
    public string Price
    {
        get => _price;
        set
        {
            if (_price != value)
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }
    }

    private string _category = "";
    public string Category
    {
        get => _category;
        set
        {
            if (_category != value)
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }
    }

    private Dictionary<int, string> _categoryMap = new();
    public Dictionary<int, string> CategoryMap
    {
        get => _categoryMap;
        set
        {
            _categoryMap = value;
            OnPropertyChanged(nameof(CategoryMap));
        }
    }

    private string _sort = "";
    public string Sort
    {
        get => _sort;
        set
        {
            if (_sort != value)
            {
                _sort = value;
                OnPropertyChanged(nameof(Sort));
            }
        }
    }

    private ObservableCollection<string> _priceSelection = new();
    public ObservableCollection<string> PriceSelection
    {
        get => _priceSelection;
        set
        {
            _priceSelection = value;
            OnPropertyChanged(nameof(PriceSelection));
        }
    }

    private ObservableCollection<string> _categorySelection = new();
    public ObservableCollection<string> CategorySelection
    {
        get => _categorySelection;
        set
        {
            _categorySelection = value;
            OnPropertyChanged(nameof(CategorySelection));
        }
    }

    private ObservableCollection<string> _sortSelection = new();
    public ObservableCollection<string> SortSelection
    {
        get => _sortSelection;
        set
        {
            _sortSelection = value;
            OnPropertyChanged(nameof(SortSelection));
        }
    }

    [RelayCommand]
    private void OnItemClick(CoffeePOS.Core.Models.Product? clickedItem)
    {
        if (clickedItem != null)
        {
            _navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
            var parameter = (clickedItem.Id, false);
            _navigationService.NavigateTo(typeof(ProductsDetailViewModel).FullName!, parameter);
        }
    }

    [RelayCommand]
    private async void AddProduct()
    {
        var product = new Product();
        var dialog = ContentDiaglogHelper.CreateProductDialog(product);
        var result = await ContentDiaglogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var contentPanel = (StackPanel)((ContentDialog)dialog).Content;

            var nameBox = (TextBox)((StackPanel)contentPanel.Children[0]).Children[1];
            var priceBox = (TextBox)((StackPanel)contentPanel.Children[1]).Children[1];
            var descriptionBox = (TextBox)((StackPanel)contentPanel.Children[2]).Children[1];
            var categoryBox = (ComboBox)((StackPanel)contentPanel.Children[3]).Children[1];

            product.Name = nameBox.Text;
            product.Price = double.TryParse(priceBox.Text, out double price) ? price : 0;
            product.Description = descriptionBox.Text;
            product.CategoryId = CategoryMap.FirstOrDefault(x => x.Value == categoryBox.SelectedItem.ToString()).Key;
            product.IsStocked = true;

            await _dao.Products.Add(product);
            await LoadProducts();
            ApplyFilters();
        }
    }


    [RelayCommand]
    private void ApplyFilters()
    {
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchName))
        {
            filtered = filtered.Where(p => p.Name.Contains(SearchName, StringComparison.OrdinalIgnoreCase));
        }

        if (Category != "All" && !String.IsNullOrEmpty(Category))
        {
            filtered = filtered.Where(p => CategoryMap.ContainsKey(p.CategoryId) && CategoryMap[p.CategoryId] == Category);
        }

        if (Price != "All" && !String.IsNullOrEmpty(Price))
        {
            filtered = filtered.Where(p => CheckPriceFilter(p.Price));
        }

        if (Sort == "Price ascending")
        {
            filtered = filtered.OrderBy(p => p.Price);
        }
        else if (Sort == "Price descending")
        {
            filtered = filtered.OrderByDescending(p => p.Price);
        }

        _filteredProducts = filtered.ToList();
        _currentPage = 1;
        UpdatePagedProducts();
    }

    [RelayCommand]
    private void RemoveFilter()
    {
        SearchName = "";
        Price = Category = Sort = null;
        ApplyFilters();
    }

    private bool CheckPriceFilter(double price)
    {
        return Price switch
        {
            "Under $2.00" => price < 2.00,
            "$2.00 - $3.99" => price >= 2.00 && price <= 3.99,
            "$4.00 - $5.99" => price >= 4.00 && price <= 5.99,
            "$6.00 - $8.00" => price >= 6.00 && price <= 8.00,
            "Over $8" => price > 8.00,
            _ => true
        };
    }

    // Pagination
    private int _currentPage = 1;
    private int _itemsPerPage = 15;
    private List<CoffeePOS.Core.Models.Product> _filteredProducts = new();

    public int ItemsPerPage
    {
        get => _itemsPerPage;
        set
        {
            _currentPage = 1;
            SetProperty(ref _itemsPerPage, value); 
            UpdatePagedProducts();
        }
    }

    private void UpdatePagedProducts()
    {
        Products.Clear();
        var pagedProducts = _filteredProducts.Skip((_currentPage - 1) * _itemsPerPage).Take(_itemsPerPage).ToList();
        pagedProducts.ForEach(Products.Add);
        OnPropertyChanged(nameof(PageIndicator));
    }

    public string PageIndicator => $"{_currentPage}/{(int)Math.Ceiling((double)_filteredProducts.Count / _itemsPerPage)}";

    [RelayCommand]
    private void NextPage()
    {
        if (_currentPage < (int)Math.Ceiling((double)_filteredProducts.Count / _itemsPerPage))
        {
            _currentPage++;
            UpdatePagedProducts();
            OnPropertyChanged(nameof(PageIndicator));
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            UpdatePagedProducts();
            OnPropertyChanged(nameof(PageIndicator));
        }
    }
}
