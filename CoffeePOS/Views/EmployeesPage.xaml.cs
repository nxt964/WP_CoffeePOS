using CoffeePOS.Core.Daos;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace CoffeePOS.Views
{
    public sealed partial class EmployeesPage : Page
    {
        public EmployeesViewModel ViewModel
        {
            get;
        }

        private readonly IDao _dao;

        public EmployeesPage()
        {
            ViewModel = App.GetService<EmployeesViewModel>();
            _dao = App.GetService<IDao>(); // Lấy IDao từ DI container
            this.InitializeComponent();
            DataContext = ViewModel;
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
            System.Diagnostics.Debug.WriteLine("[DEBUG] EmployeesPage.AddButton_Click: Navigating to AddEmployeePage...");
            Frame.Navigate(typeof(AddEmployeePage));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                if (int.TryParse(button.Tag.ToString(), out int employeeId))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] EmployeesPage.EditButton_Click: Navigating to EditEmployeePage with EmployeeId = {employeeId}");
                    Frame.Navigate(typeof(EditEmployeePage), employeeId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] EmployeesPage.EditButton_Click: Invalid EmployeeId = {button.Tag}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] EmployeesPage.EditButton_Click: Button or Tag is null");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null || !int.TryParse(button.Tag.ToString(), out int employeeId))
            {
                Debug.WriteLine("[ERROR] EmployeesPage.DeleteButton_Click: Invalid EmployeeId");
                return;
            }

            Debug.WriteLine($"[DEBUG] EmployeesPage.DeleteButton_Click: Attempting to delete EmployeeId = {employeeId}");

            // Hiển thị hộp thoại xác nhận
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Are you sure you want to delete employee with ID {employeeId}?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                Debug.WriteLine("[DEBUG] EmployeesPage.DeleteButton_Click: Deletion canceled by user");
                return; // Người dùng hủy xóa
            }

            try
            {
                // Kiểm tra xem nhân viên có tồn tại trong cơ sở dữ liệu không
                var employeeExists = await _dao.Employees.GetById(employeeId);
                if (employeeExists == null)
                {
                    Debug.WriteLine($"[ERROR] EmployeesPage.DeleteButton_Click: EmployeeId {employeeId} not found in database");
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Employee with ID {employeeId} not found in database.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                // Xóa khỏi cơ sở dữ liệu
                Debug.WriteLine($"[DEBUG] EmployeesPage.DeleteButton_Click: Calling _dao.Employees.Delete for EmployeeId = {employeeId}");
                await _dao.Employees.Delete(employeeId);
                Debug.WriteLine($"[DEBUG] EmployeesPage.DeleteButton_Click: Calling _dao.SaveChangesAsync for EmployeeId = {employeeId}");
                await _dao.SaveChangesAsync();

                ViewModel.DeleteCommand.Execute(employeeId);

                Debug.WriteLine($"[DEBUG] EmployeesPage.DeleteButton_Click: Successfully deleted EmployeeId {employeeId} from database");

                // Hiển thị thông báo thành công
                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = $"Employee with ID {employeeId} has been deleted successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] EmployeesPage.DeleteButton_Click: Failed to delete EmployeeId {employeeId}. Exception: {ex.Message}");
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to delete employee: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}