using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using CoffeePOS.Core.Models;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using CoffeePOS.Core.Interfaces;
using System.Diagnostics;

namespace CoffeePOS.Helpers;
public class ContentDialogHelper
{
    private readonly IDao _dao;

    public ContentDialogHelper()
    {
        _dao = App.GetService<IDao>();
    }
    
    public async Task<ContentDialogResult> ShowDialogWithSlideIn(ContentDialog dialog)
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

    public ContentDialog CreateCategoryDialog(string Name = "", string Description = "", bool isNew = true)
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

    private async Task<string> PickImageAsync()
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var copiedFile = await file.CopyAsync(localFolder, file.Name, NameCollisionOption.ReplaceExisting);
            return copiedFile.Path;
        }

        return string.Empty;
    }

    public ContentDialog CreateProductDialog(Product product, bool isNew = true)
    {  
        var contentPanel = new StackPanel();

        contentPanel.Children.Add(new StackPanel {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 15),
            Children = {
                new TextBlock { Text = "Name:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "NameBox", Text=product.Name, PlaceholderText = "Product Name", Width = 200 }
            }
        });
    
        contentPanel.Children.Add(new StackPanel {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 15),
            Children = {
                new TextBlock { Text = "Price ($):", Width = 80, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "PriceBox", Text=product.Price.ToString(), PlaceholderText = "Ex: 2.5", Width = 200 }
            }
        });
        
        contentPanel.Children.Add(new StackPanel {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 15),
            Children = {
                new TextBlock { Text = "Description:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
                new TextBox { Name = "DescriptionBox", Text=product.Description, PlaceholderText = "Description...", Width = 200 }
            }
        });

        var categoryBox = new ComboBox { Name = "CategoryBox", Width = 200 };
        var categories = _dao.Categories.GetAll().Result.ToList();
        categories.ForEach(c => categoryBox.Items.Add(c.Name));
        if (!string.IsNullOrEmpty(product.CategoryId.ToString()))
        {
            var selectedCategory = categories.FirstOrDefault(c => c.Id == product.CategoryId);
            if (selectedCategory != null)
            {
                categoryBox.SelectedItem = selectedCategory.Name;
            }
        }

        contentPanel.Children.Add(new StackPanel { 
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 15),
            Children = {
                new TextBlock { Text = "Category:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
                categoryBox
            }
        });

        var imagePreview = new Image { 
            Width = 150, 
            Height = 150,
            Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill,
            Source = new BitmapImage(new Uri(
                string.IsNullOrEmpty(product.Image) 
                ? "ms-appx:///Assets/ProductImageDefault.png"
                : product.Image))
        };

        var pickImageButton = new Button
        {
            Content = "Choose Image"
        };

        pickImageButton.Click += async (s, e) =>
        {
            var selectedImagePath = await PickImageAsync();
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                imagePreview.Source = new BitmapImage(new Uri($"file:///{selectedImagePath}"));
                product.Image = selectedImagePath;
            }
        };

        contentPanel.Children.Add(new StackPanel {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 15),
            Children = {
                    new TextBlock { Text = "Image:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
                    pickImageButton
                }
        });

        contentPanel.Children.Add(new Border {
            Padding = new Thickness(5, 10, 5, 10),
            CornerRadius = new CornerRadius(7),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(230, 230, 230, 230)),
            BorderThickness = new Thickness(1),
            Child = imagePreview
        });

            

        return new ContentDialog 
        {
            Title = isNew ? "Add Product" : "Edit Product",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Content = contentPanel
        };
    }


}
