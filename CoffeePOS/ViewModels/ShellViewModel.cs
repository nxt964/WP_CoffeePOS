using CoffeePOS.Contracts.Services;
using CoffeePOS.Views;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

namespace CoffeePOS.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    // Perform logout actions
    public void PerformLogout()
    {
        // Clear any user data, authentication tokens, etc.
        // For example:
        // App.Current.Properties["IsLoggedIn"] = false;
        // App.Current.Properties["UserToken"] = null;

        // If you have a user service or authentication service, use it here
        // For example:
        // var authService = App.GetService<IAuthenticationService>();
        // authService.Logout();

        // You could add any additional logout logic here
    }
}