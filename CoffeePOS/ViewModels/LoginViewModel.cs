using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Helpers;
using CoffeePOS.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace CoffeePOS.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IDao _dao;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool rememberMe;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string successMessage;

        private DispatcherTimer _timer;

        public LoginViewModel(IDao dao)
        {
            _dao = dao;
        }

        // Khởi tạo ViewModel (không cần kiểm tra Remember Me nữa)
        public async Task InitializeAsync()
        {
            var savedData = await RememberMeHelper.LoadAsync();
            if (savedData != null)
            {
                Username = savedData.Username;
                Password = savedData.Password;
                RememberMe = true;
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
                var user = await _dao.Users.Login(Username, Password);

                if (user != null)
                {
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
                            if (RememberMe)
                            {
                                await RememberMeHelper.SaveAsync(new RememberMeData
                                {
                                    Username = Username,
                                    Password = Password
                                });
                            }
                            else
                            {
                                RememberMeHelper.Delete();
                            }
                            StartTrialMonitor(user);
                        }
                    }

                    ErrorMessage = string.Empty;
                    SuccessMessage = "Log in successfully!";

                    // Chuyển sang màn hình chính
                    UIElement shell = App.GetService<ShellPage>();
                    App.MainWindow.Content = shell ?? new Frame();
                }
                else
                {
                    ErrorMessage = "Invalid Username or Password!";
                    SuccessMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Something went wrong: {ex.Message}";
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
                Text = "Create a free trial account to test the application. This account will expire in 2 minutes.",
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

            var errorText = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Red),
                Margin = new Thickness(0, 10, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            panel.Children.Add(infoText);
            panel.Children.Add(usernameBox);
            panel.Children.Add(passwordBox);
            panel.Children.Add(confirmPasswordBox);
            panel.Children.Add(errorText);

            dialog.Content = panel;

            dialog.PrimaryButtonClick += async (s, args) =>
            {
                var inputUsername = usernameBox.Text?.Trim();
                var inputPassword = passwordBox.Password?.Trim();
                var confirmPassword = confirmPasswordBox.Password?.Trim();

                if (string.IsNullOrWhiteSpace(inputUsername) ||
                    string.IsNullOrWhiteSpace(inputPassword) ||
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    errorText.Text = "Please fill in all fields to create account!";
                    args.Cancel = true;
                    return;
                }

                if (inputPassword != confirmPassword)
                {
                    errorText.Text = "Password does not match!";
                    args.Cancel = true;
                    return;
                }

                var trialUser = new User
                {
                    Username = inputUsername,
                    Password = inputPassword,
                    ExpireAt = DateTime.UtcNow.AddSeconds(120)
                };

                var isAdded = await _dao.Users.AddTrialUser(trialUser);
                if (isAdded == null)
                {
                    errorText.Text = "Username already exists!";
                    args.Cancel = true;
                    return;
                }

                ErrorMessage = string.Empty;
                SuccessMessage = "Trial account created successfully!";
            };

            await dialog.ShowAsync();
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
}
