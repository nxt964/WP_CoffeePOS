using CoffeePOS.Core.Daos;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace CoffeePOS.Views
{
    public sealed partial class CustomersPage : Page
    {
        public CustomersViewModel ViewModel
        {
            get;
        }

        private readonly IDao _dao;

        public CustomersPage()
        {
            ViewModel = App.GetService<CustomersViewModel>();
            _dao = App.GetService<IDao>();
            this.InitializeComponent();
            DataContext = ViewModel;
            // Truyền XamlRoot vào ViewModel
            ViewModel.SetXamlRoot(this.XamlRoot);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.OnNavigatedTo(e.Parameter);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.OnNavigatedFrom();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CustomersPage.AddButton_Click: Navigating to AddCustomerPage...");
            Frame.Navigate(typeof(AddCustomerPage));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                if (int.TryParse(button.Tag.ToString(), out int customerId))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] CustomersPage.EditButton_Click: Navigating to EditCustomerPage with CustomerId = {customerId}");
                    Frame.Navigate(typeof(EditCustomerPage), customerId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] CustomersPage.EditButton_Click: Invalid CustomerId = {button.Tag}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] CustomersPage.EditButton_Click: Button or Tag is null");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null || !int.TryParse(button.Tag.ToString(), out int customerId))
            {
                Debug.WriteLine("[ERROR] CustomersPage.DeleteButton_Click: Invalid CustomerId");
                return;
            }

            Debug.WriteLine($"[DEBUG] CustomersPage.DeleteButton_Click: Attempting to delete CustomerId = {customerId}");

            // Hiển thị hộp thoại xác nhận
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Be sure that the cusomter deleted with ID {customerId} has no order?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                Debug.WriteLine("[DEBUG] CustomersPage.DeleteButton_Click: Deletion canceled by user");
                return;
            }

            // Gọi lệnh Delete từ ViewModel
            await ViewModel.DeleteCommand.ExecuteAsync(customerId);
        }
    }
}