using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace CoffeePOS.Views;

public sealed partial class AddCustomerPage : Page
{
    public AddCustomerViewModel ViewModel
    {
        get;
    }

    public AddCustomerPage()
    {
        ViewModel = App.GetService<AddCustomerViewModel>();
        InitializeComponent();
        ViewModel.SetXamlRoot(this.XamlRoot);
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