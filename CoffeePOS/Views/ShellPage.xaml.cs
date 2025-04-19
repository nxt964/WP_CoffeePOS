using CoffeePOS.Contracts.Services;
using CoffeePOS.Helpers;
using CoffeePOS.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Windows.System;

namespace CoffeePOS.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        // Điều hướng đến DashboardPage mặc định
        ViewModel.NavigationService.NavigateTo(typeof(DashboardViewModel).FullName!);
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    // Handler for the logout button tap event
    private async void LogoutButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        // Create and configure the confirmation dialog
        ContentDialog logoutDialog = new ContentDialog
        {
            Title = "Logout Confirmation",
            Content = "Are you sure you want to log out of the system?",
            PrimaryButtonText = "Logout",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        // Show the dialog and get the result
        ContentDialogResult result = await logoutDialog.ShowAsync();

        // If the user confirmed, proceed with logout
        if (result == ContentDialogResult.Primary)
        {
            // Perform logout actions
            // For example, clear credentials, user session, etc.
            ViewModel.PerformLogout();

            // Navigate to login page
            // Assuming you have a LoginViewModel or similar
            //ViewModel.NavigationService.NavigateTo("CoffeePOS.ViewModels.LoginViewModel");
            App.MainWindow.Content = new Frame();
            if (App.MainWindow.Content is Frame frame)
            {
                frame.Navigate(typeof(LoginPage));
            }
        }
    }
}