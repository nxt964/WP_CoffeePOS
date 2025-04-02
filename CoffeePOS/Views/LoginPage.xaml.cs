// Views/LoginPage.xaml.cs
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
        this.DataContext = App.GetService<LoginViewModel>(); // Liên kết với LoginViewModel
    }
}