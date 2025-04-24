using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace CoffeePOS.Views;

public sealed partial class EditCustomerPage : Page
{
    public EditCustomerViewModel ViewModel
    {
        get;
    }

    public EditCustomerPage()
    {
        ViewModel = App.GetService<EditCustomerViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int customerId)
        {
            ViewModel.SetXamlRoot(this.XamlRoot);
            ViewModel.OnNavigatedTo(customerId);
        }
    }

    private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            string filteredText = new string(textBox.Text.Where(char.IsDigit).ToArray());
            if (textBox.Text != filteredText)
            {
                int caretIndex = textBox.SelectionStart;
                textBox.Text = filteredText;
                textBox.SelectionStart = caretIndex > filteredText.Length ? filteredText.Length : caretIndex;
            }
        }
    }
}