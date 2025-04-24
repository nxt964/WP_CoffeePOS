using System.Collections.ObjectModel;

using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Helpers;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.ViewModels;

public partial class CategoriesViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;

    [ObservableProperty]
    private CategoryProducts? selected;

    private ContentDialogHelper ContentDialogHelper { get; } = new ContentDialogHelper();

    public ObservableCollection<CategoryProducts> Categories { get; private set; } = new ObservableCollection<CategoryProducts>();

    public CategoriesViewModel(IDao dao)
    {
        _dao = dao;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadCategories();
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        Selected ??= Categories.First();
    }

    private async Task LoadCategories()
    {
        Categories.Clear();
        var categories = (await _dao.Categories.GetAll()).ToList();
        var products = (await _dao.Products.GetAll()).ToList();

        categories.ForEach(category =>
        {
            Categories.Add(new CategoryProducts
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Products = products.Where(p => p.CategoryId == category.Id).ToList()
            });
        });
    }

    [RelayCommand]
    public async Task AddCategory()
    {
        var dialog = ContentDialogHelper.CreateCategoryDialog();

        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var outerPanel = (StackPanel)dialog.Content;
            var nameBox = (TextBox)((StackPanel)outerPanel.Children[0]).Children[1];
            var descriptionBox = (TextBox)((StackPanel)outerPanel.Children[2]).Children[1];

            var newCategory = new Category { Id = Categories.Count + 1, Name = nameBox.Text, Description = descriptionBox.Text };
            await _dao.Categories.Add(newCategory);
            await LoadCategories();
            EnsureItemSelected();
        }
    }

    [RelayCommand]
    public async Task EditCategory()
    {
        if (Selected == null) return;

        var dialog = ContentDialogHelper.CreateCategoryDialog(Selected.Name, Selected.Description, false);

        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var outerPanel = (StackPanel)dialog.Content;
            var nameBox = (TextBox)((StackPanel)outerPanel.Children[0]).Children[1];
            var descriptionBox = (TextBox)((StackPanel)outerPanel.Children[2]).Children[1];

            await _dao.Categories.Update(new Category { Id = Selected.Id, Name = nameBox.Text, Description = descriptionBox.Text });
            await LoadCategories();
            EnsureItemSelected();
        }
    }


    [RelayCommand]
    public async Task DeleteCategory()
    {
        if (Selected == null) return;

        var dialog = new ContentDialog
        {
            Title = "Delete Category",
            Content = $"Are you sure you want to delete '{Selected.Name}'?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
        var result = await ContentDialogHelper.ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            await _dao.Categories.Delete(Selected.Id);
            await LoadCategories();
            EnsureItemSelected();
        }
    }

}
