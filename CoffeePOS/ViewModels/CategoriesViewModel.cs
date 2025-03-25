using System.Collections.ObjectModel;

using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Contracts.Services;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace CoffeePOS.ViewModels;

public partial class CategoriesViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDao _dao;

    [ObservableProperty]
    private CategoryProducts? selected;

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

    private async Task<ContentDialogResult> ShowDialogWithSlideIn(ContentDialog dialog)
    {
        dialog.Opacity = 0;

        var transform = new CompositeTransform { TranslateY = 100 };
        dialog.RenderTransform = transform;

        var storyboard = new Storyboard();

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300))
        };
        Storyboard.SetTarget(fadeIn, dialog);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");

        var slideIn = new DoubleAnimation
        {
            From = 100, 
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(slideIn, transform);
        Storyboard.SetTargetProperty(slideIn, "TranslateY");

        storyboard.Children.Add(fadeIn);
        storyboard.Children.Add(slideIn);
        storyboard.Begin();

        return await dialog.ShowAsync();
    }

    private ContentDialog CreateContentDialog(string Name = "", string Description = "", bool isNew=true)
    {
        var contentPanel = new StackPanel();
        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                new TextBlock { Text = "Name:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "NameBox", Text=Name, PlaceholderText = "Category Name", Width = 200 }
            }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Description:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "DescriptionBox", Text=Description, PlaceholderText = "Description", Width = 200 }
            }
        });

        return new ContentDialog
        {
            Title = isNew ? "Add Category" : "Edit Category",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Content = contentPanel
        };
    }

    [RelayCommand]
    public async Task AddCategory()
    {
        var dialog = CreateContentDialog();

        var result = await ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var outerPanel = (StackPanel)dialog.Content;
            var nameBox = (TextBox)((StackPanel)outerPanel.Children[0]).Children[1];
            var descriptionBox = (TextBox)((StackPanel)outerPanel.Children[1]).Children[1];

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

        var dialog = CreateContentDialog(Selected.Name, Selected.Description, false);

        var result = await ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            var outerPanel = (StackPanel)dialog.Content;
            var nameBox = (TextBox)((StackPanel)outerPanel.Children[0]).Children[1];
            var descriptionBox = (TextBox)((StackPanel)outerPanel.Children[1]).Children[1];

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
        var result = await ShowDialogWithSlideIn(dialog);

        if (result == ContentDialogResult.Primary)
        {
            await _dao.Categories.Delete(Selected.Id);
            await LoadCategories();
            EnsureItemSelected();
        }
    }

}
