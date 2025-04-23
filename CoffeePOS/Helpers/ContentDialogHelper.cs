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
using CoffeePOS.Services;
using Microsoft.UI;

namespace CoffeePOS.Helpers;
public class ContentDialogHelper
{
    private readonly IDao _dao;
    private readonly CloudinaryService _cloudinaryService;

    public ContentDialogHelper()
    {
        _dao = App.GetService<IDao>();
        _cloudinaryService = App.GetService<CloudinaryService>();
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

    public TextBlock ErrorText(string fieldName, string errorMessage)
    {
        return new TextBlock
        {
            Name = $"{fieldName}ErrorText",
            Text = errorMessage,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
            FontSize = 12,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(84, 2, 0, 0)
        };
    }

    public bool ValidateTextBox(TextBox textBox, TextBlock errorTextBlock)
    {
        Console.WriteLine($"Validating {textBox.Name}: {textBox.Text}");
        var isValid = !string.IsNullOrWhiteSpace(textBox.Text);
        if (!isValid)
        {
            textBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
            textBox.BorderThickness = new Thickness(1);
            errorTextBlock.Visibility = Visibility.Visible;
        } else
        {
            errorTextBlock.Visibility = Visibility.Collapsed;
            textBox.ClearValue(TextBox.BorderBrushProperty);
        }

        return isValid;
    }

    private bool ValidatePriceBox(TextBox textBox, TextBlock errorText)
    {
        var isValid = double.TryParse(textBox.Text, out double result) && result > 0;

        if (!isValid)
        {
            textBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
            textBox.BorderThickness = new Thickness(1);
            errorText.Visibility = Visibility.Visible;
        }
        else
        {
            textBox.ClearValue(TextBox.BorderBrushProperty);
            errorText.Visibility = Visibility.Collapsed;
        }

        return isValid;
    }

    private bool ValidateComboBox(ComboBox box, TextBlock errorText)
    {
        var isValid = box.SelectedItem != null;
        if (!isValid)
        {
            box.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
            box.BorderThickness = new Thickness(1);
            errorText.Visibility = Visibility.Visible;
        }
        else
        {
            box.ClearValue(ComboBox.BorderBrushProperty);
            errorText.Visibility = Visibility.Collapsed;
        }
        return isValid;
    }

    public ContentDialog CreateCategoryDialog(string Name = "", string Description = "", bool isNew = true)
    {
        var nameBox = new TextBox
        {
            Name = "NameBox",
            Text = Name,
            PlaceholderText = "Category Name",
            Width = 200
        };

        var nameErrorText = ErrorText("Name", "Name is required!");

        var descriptionBox = new TextBox
        {
            Name = "DescriptionBox",
            Text = Description,
            PlaceholderText = "Description",
            Width = 200
        };
        var descriptionErrorText = ErrorText("Description", "Description is required!");

        var contentPanel = new StackPanel();

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
        {
            new TextBlock { Text = "Name:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            nameBox
        }
        });
        contentPanel.Children.Add(nameErrorText);


        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 15, 0, 0),
            Children =
        {
            new TextBlock { Text = "Description:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            descriptionBox
        }
        });
        contentPanel.Children.Add(descriptionErrorText);

        var dialog = new ContentDialog
        {
            Title = isNew ? "Add Category" : "Edit Category",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Content = contentPanel
        };

        dialog.PrimaryButtonClick += (s, args) =>
        {
            var valid = ValidateTextBox(nameBox, nameErrorText) 
                      & ValidateTextBox(descriptionBox, descriptionErrorText);

            if (!valid)
            {
                args.Cancel = true;
                return;
            }
        };

        return dialog;
    }



    private async Task<StorageFile?> PickImageAsync()
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

        return await picker.PickSingleFileAsync();
    }

    private async Task<string> UploadToCloudinaryAsync(StorageFile file)
    {
        if (file == null) return string.Empty;

        // Cloudinary cần đường dẫn vật lý, nên copy file tạm
        var tempFile = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.GenerateUniqueName);

        return await _cloudinaryService.UploadImageAsync(tempFile.Path);
    }


    public ContentDialog CreateProductDialog(Product product, bool isNew = true)
    {
        var contentPanel = new StackPanel();

        // === Tạo các control cần dùng ===
        var nameBox = new TextBox { Text = product.Name, PlaceholderText = "Product Name", Width = 200 };
        var priceBox = new TextBox { Text = product.Price != 0 ? product.Price.ToString() : "", PlaceholderText = "Ex: 2.5", Width = 200 };
        var descriptionBox = new TextBox { Text = product.Description, PlaceholderText = "Description...", Width = 200 };
        var categoryBox = new ComboBox { Width = 200, PlaceholderText = "Select category" };
        var inStockCheckBox = new CheckBox
        {
            IsChecked = product.IsStocked,
            Content = product.IsStocked ? "Available" : "Unavailable"
        };
        var imagePreview = new Image
        {
            Width = 150,
            Height = 150,
            Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill,
            Source = new BitmapImage(new Uri(
                string.IsNullOrEmpty(product.Image)
                ? "ms-appx:///Assets/ProductImageDefault.png"
                : product.Image))
        };

        // === Gán sự kiện cho checkbox ===
        inStockCheckBox.Checked += (s, e) => inStockCheckBox.Content = "Available";
        inStockCheckBox.Unchecked += (s, e) => inStockCheckBox.Content = "Unavailable";

        // === Gán sự kiện chọn ảnh ===
        var pickImageButton = new Button { Content = "Choose Image" };
        pickImageButton.Click += async (s, e) =>
        {
            var file = await PickImageAsync();
            if (file != null)
            {
                var imageUrl = await UploadToCloudinaryAsync(file);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    imagePreview.Source = new BitmapImage(new Uri(imageUrl));
                    product.Image = imageUrl;
                }
            }
        };

        // === Lấy danh sách category ===
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

        // === Error TextBlocks ===
        var nameErrorText = ErrorText("Name", "Product name is required!");
        var priceErrorText = ErrorText("Price", "Price must be a valid positive number!");
        var descriptionErrorText = ErrorText("Description", "Description is required!");
        var categoryErrorText = ErrorText("Category", "Please select a category!");

        // === UI Layout ===
        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = {
            new TextBlock { Text = "Name:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            nameBox
        }
        });
        contentPanel.Children.Add(nameErrorText);

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 15, 0, 0),
            Children = {
            new TextBlock { Text = "Price ($):", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            priceBox
        }
        });
        contentPanel.Children.Add(priceErrorText);

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 15, 0, 0),
            Children = {
            new TextBlock { Text = "Description:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            descriptionBox
        }
        });
        contentPanel.Children.Add(descriptionErrorText);

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 15, 0, 0),
            Children = {
            new TextBlock { Text = "Category:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            categoryBox
        }
        });
        contentPanel.Children.Add(categoryErrorText);

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 15, 0, 0),
            Children = {
            new TextBlock { Text = "In Stock:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            inStockCheckBox
        }
        });

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 15, 0, 0),
            Children = {
            new TextBlock { Text = "Image:", Width = 80, VerticalAlignment = VerticalAlignment.Center },
            pickImageButton
        }
        });

        contentPanel.Children.Add(new Border
        {
            Padding = new Thickness(5, 10, 5, 10),
            Margin = new Thickness(0, 15, 0, 0),
            CornerRadius = new CornerRadius(7),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(230, 230, 230, 230)),
            BorderThickness = new Thickness(1),
            Child = imagePreview
        });

        // === ContentDialog ===
        var dialog = new ContentDialog
        {
            Title = isNew ? "Add Product" : "Edit Product",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Content = contentPanel
        };

        dialog.PrimaryButtonClick += (s, args) =>
        {
            var isValid = ValidateTextBox(nameBox, nameErrorText)
                       & ValidatePriceBox(priceBox, priceErrorText)
                       & ValidateTextBox(descriptionBox, descriptionErrorText)
                       & ValidateComboBox(categoryBox, categoryErrorText);

            if (!isValid)
            {
                args.Cancel = true;
            }
        };

        return dialog;
    }


    public ContentDialog CreateTableDialog(Table table = null, bool isNew = true)
    {
        table ??= new Table { Status = "Available" };

        var contentPanel = new StackPanel { Spacing = 10 };

        // Table Number input
        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
        {
            new TextBlock { Text = "Table Number:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
            new TextBox { Text = table.TableNumber, PlaceholderText = "Enter table number", Width = 200 }
        }
        });
          
        // Status selection
        var statusCombo = new ComboBox
        {
            Width = 200,
            ItemsSource = new List<string> { "Available", "Occupied", "Reserved", "Maintenance" }
        };

        if (!string.IsNullOrEmpty(table.Status))
        {
            statusCombo.SelectedItem = table.Status;
        }
        else
        {
            statusCombo.SelectedIndex = 0;
        }

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
        {
            new TextBlock { Text = "Status:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
            statusCombo
        }
        });

        return new ContentDialog
        {
            Title = isNew ? "Add New Table" : "Edit Table",
            PrimaryButtonText = isNew ? "Add" : "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Content = contentPanel
        };
    }

    public ContentDialog CreateTableStatusDialog(Table table)
    {
        var contentPanel = new StackPanel { Spacing = 10 };

        contentPanel.Children.Add(new TextBlock { Text = $"Current Status: {table.Status}" });

        var statusCombo = new ComboBox
        {
            ItemsSource = new List<string> { "Available", "Occupied", "Reserved", "Maintenance" },
            SelectedItem = table.Status,
            Width = 200,
            Margin = new Thickness(0, 5, 0, 0)
        };

        contentPanel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
        {
            new TextBlock { Text = "New Status:", Width = 100, VerticalAlignment = VerticalAlignment.Center },
            statusCombo
        }
        });

        return new ContentDialog
        {
            Title = "Change Table Status",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Content = contentPanel
        };
    }

    public ContentDialog CreateDeleteTableConfirmationDialog(Table table)
    {
        return new ContentDialog
        {
            Title = "Confirm Deletion",
            Content = $"Are you sure you want to delete table {table.TableNumber}?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
    }

    public ContentDialog CreateErrorDialog(string message)
    {
        return new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
    }


}







