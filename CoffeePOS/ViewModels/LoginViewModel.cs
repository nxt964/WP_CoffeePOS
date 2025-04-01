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
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string errorMessage;

    public LoginViewModel(IDao dao, INavigationService navigationService)
    {
        _dao = dao;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please input Username and Password";
            return;
        }

        try
        {
            // Using the IUserRepository.Login method
            var user = await _dao.Users.Login(Username, Password);

            if (user != null)
            {
                // Clear error message on successful login
                ErrorMessage = string.Empty;

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
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Somethings went wrong: {ex.Message}";
        }
    }
}