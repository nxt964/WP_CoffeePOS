// ViewModels/LoginViewModel.cs
using CoffeePOS.Contracts.Services;
using CoffeePOS.Core.Daos;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace CoffeePOS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IDao _dao;

    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private string successMessage;

    [ObservableProperty]
    private bool rememberMe;

    private DispatcherTimer _timer;

    public LoginViewModel(IDao dao)
    {
        _dao = dao;

        var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
        if (settings.Values.TryGetValue("RememberMe", out var remember) && (bool)remember)
        {
            RememberMe = true;
            Username = settings.Values["SavedUsername"]?.ToString();
            Password = settings.Values["SavedPassword"]?.ToString();
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please input Username and Password";
            SuccessMessage = string.Empty;
            return;
        }

        try
        {
            // Using the IUserRepository.Login method
            var user = await _dao.Users.Login(Username, Password);

            if (user != null)
            {
                // Check if the user is expired
                if (user.ExpireAt != null)
                {
                    if (user.ExpireAt <= DateTime.Now)
                    {
                        ErrorMessage = "Your trial account has expired!";
                        SuccessMessage = string.Empty;
                        return;
                    }
                    else
                    {
                        StartTrialMonitor(user);
                    }
                }

                // Clear error message on successful login
                ErrorMessage = string.Empty;
                SuccessMessage = "Log in successfully!";

                // Save the username and password if RememberMe is checked
                if (RememberMe)
                {
                    var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    settings.Values["RememberMe"] = true;
                    settings.Values["SavedUsername"] = Username;
                    settings.Values["SavedPassword"] = Password;
                }
                else
                {
                    var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    settings.Values.Remove("RememberMe");
                    settings.Values.Remove("SavedUsername");
                    settings.Values.Remove("SavedPassword");
                }

                // Navigate to the main application shell

                //_navigationService.NavigateTo(typeof(ProductViewModel).FullName!);
                // Điều hướng đến ShellPage thay vì DashboardViewModel
                //_navigationService.NavigateTo(typeof(ShellViewModel).FullName!);
                UIElement _shell = App.GetService<ShellPage>();
                App.MainWindow.Content = _shell ?? new Frame();
            }
            else
            {
                ErrorMessage = "Invalid Username or Password!";
                SuccessMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Somethings went wrong: {ex.Message}";
            SuccessMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task TrialAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Free trial mode",
            XamlRoot = App.MainWindow.Content.XamlRoot,
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var panel = new StackPanel();

        var infoText = new TextBlock
        {
            Text = "Create a free trial account to test the application. This account will expire in 30 seconds.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var usernameBox = new TextBox
        {
            Header = "Username",
            Margin = new Thickness(0, 0, 0, 10)
        };

        var passwordBox = new PasswordBox
        {
            Header = "Password",
            Margin = new Thickness(0, 0, 0, 10)
        };

        var confirmPasswordBox = new PasswordBox
        {
            Header = "Confirm Password",
            Margin = new Thickness(0, 0, 0, 10)
        };

        panel.Children.Add(infoText);
        panel.Children.Add(usernameBox);
        panel.Children.Add(passwordBox);
        panel.Children.Add(confirmPasswordBox);

        dialog.Content = panel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var inputUsername = usernameBox.Text?.Trim();
            var inputPassword = passwordBox.Password?.Trim();
            var confirmPassword = confirmPasswordBox.Password?.Trim();

            if (string.IsNullOrWhiteSpace(inputUsername) || string.IsNullOrWhiteSpace(inputPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ErrorMessage = "Please fill in all fields to create account!";
                SuccessMessage = string.Empty;
                return;
            }

            if (inputPassword != confirmPassword)
            {
                ErrorMessage = "Password does not match!";
                SuccessMessage = string.Empty;
                return;
            }

            var trialUser = new User
            {
                Username = inputUsername,
                Password = inputPassword,
                ExpireAt = DateTime.UtcNow.AddSeconds(30) // dùng UTC cho chuẩn so sánh
            };

            var isAdded = await _dao.Users.AddTrialUser(trialUser);
            if (isAdded == null)
            {
                ErrorMessage = "Username already exists!";
                SuccessMessage = string.Empty;
                return;
            }
            else
            {
                ErrorMessage = string.Empty;
                SuccessMessage = "Trial account created successfully!";
                return;
            }
        }
    }

    private void StartTrialMonitor(User user)
    {
        if (user.ExpireAt == null)
            return;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += async (s, e) =>
        {
            if (user.ExpireAt <= DateTime.Now)
            {
                _timer.Stop();

                var dialog = new ContentDialog
                {
                    Title = "Trial Expired",
                    Content = "Your trial account has expired!",
                    PrimaryButtonText = "Exit",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    App.MainWindow.Content = App.GetService<LoginPage>();
                }
            }
        };

        _timer.Start();
    }
}