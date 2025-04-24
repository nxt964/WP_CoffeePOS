using CoffeePOS.Core.Interfaces;
using CoffeePOS.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoffeePOS.Views;

public sealed partial class OrderPage : Page
{
    public OrderViewModel ViewModel
    {
        get;
    }

    public OrderPage()
    {
        ViewModel = App.GetService<OrderViewModel>();
        this.DataContext = ViewModel;
        InitializeComponent();
        ViewModel.SetXamlRoot(this.XamlRoot);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[DEBUG] OrderPage.AddButton_Click: Navigating to AddOrderPage...");
        Frame.Navigate(typeof(AddOrderPage));
    }

    private void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag != null)
        {
            if (int.TryParse(button.Tag.ToString(), out int orderId))
            {
                Debug.WriteLine($"[DEBUG] OrderPage.ViewButton_Click: Navigating to DetailOrderPage with OrderId = {orderId}");
                Frame.Navigate(typeof(DetailOrderPage), orderId);
            }
            else
            {
                Debug.WriteLine($"[ERROR] OrderPage.ViewButton_Click: Invalid OrderId = {button.Tag}");
            }
        }
        else
        {
            Debug.WriteLine("[ERROR] OrderPage.ViewButton_Click: Button or Tag is null");
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag != null)
        {
            if (int.TryParse(button.Tag.ToString(), out int orderId))
            {
                Debug.WriteLine($"[DEBUG] OrderPage.DeleteButton_Click: Attempting to delete OrderId = {orderId}");

                // Create a TaskCompletionSource to await the async delegate
                var tcs = new TaskCompletionSource<bool>();

                // Ensure dialog creation runs on UI thread
                var enqueueResult = DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
                {
                    try
                    {
                        // Validate XamlRoot
                        XamlRoot xamlRoot = this.XamlRoot;
                        if (xamlRoot == null)
                        {
                            Debug.WriteLine("[ERROR] OrderPage.DeleteButton_Click: this.XamlRoot is null, attempting to use Window.Current XamlRoot");
                            var currentWindow = Window.Current;
                            if (currentWindow?.Content != null)
                            {
                                xamlRoot = currentWindow.Content.XamlRoot;
                            }
                        }

                        if (xamlRoot == null)
                        {
                            Debug.WriteLine("[ERROR] OrderPage.DeleteButton_Click: No valid XamlRoot available, cannot show dialog");
                            tcs.SetResult(false);
                            return;
                        }

                        // Validate styles
                        var accentStyle = Application.Current.Resources["AccentButtonStyle"] as Style;
                        var redStyle = Application.Current.Resources["RedButtonStyle"] as Style;

                        Debug.WriteLine($"[DEBUG] OrderPage.DeleteButton_Click: AccentButtonStyle is {(accentStyle != null ? "available" : "null")}, RedButtonStyle is {(redStyle != null ? "available" : "null")}");

                        // Create dialog
                        var confirmDialog = new ContentDialog
                        {
                            Title = "Confirm Delete",
                            Content = $"Are you sure you want to delete Order ID {orderId}?",
                            PrimaryButtonText = "Delete",
                            CloseButtonText = "Cancel",
                            DefaultButton = ContentDialogButton.Close,
                            XamlRoot = xamlRoot
                        };

                        try
                        {
                            var result = await confirmDialog.ShowAsync();
                            if (result != ContentDialogResult.Primary)
                            {
                                Debug.WriteLine($"[DEBUG] OrderPage.DeleteButton_Click: Deletion canceled for OrderId = {orderId}");
                                tcs.SetResult(false);
                                return;
                            }

                            Debug.WriteLine($"[DEBUG] OrderPage.DeleteButton_Click: Confirmed deletion for OrderId = {orderId}");
                            await ViewModel.DeleteCommand.ExecuteAsync(orderId.ToString());
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] OrderPage.DeleteButton_Click: Failed to show or process dialog. Exception: {ex.Message}");
                            tcs.SetResult(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] OrderPage.DeleteButton_Click: Unexpected error in dialog handling. Exception: {ex.Message}");
                        tcs.SetResult(false);
                    }
                });

                Debug.WriteLine($"[DEBUG] OrderPage.DeleteButton_Click: DispatcherQueue.TryEnqueue result: {enqueueResult}");

                if (!enqueueResult)
                {
                    Debug.WriteLine("[ERROR] OrderPage.DeleteButton_Click: Failed to enqueue dialog creation on UI thread");
                    return;
                }

                // Await the dialog operation
                await tcs.Task;
            }
            else
            {
                Debug.WriteLine($"[ERROR] OrderPage.DeleteButton_Click: Invalid OrderId = {button.Tag}");
            }
        }
        else
        {
            Debug.WriteLine("[ERROR] OrderPage.DeleteButton_Click: Button or Tag is null");
        }
    }
}