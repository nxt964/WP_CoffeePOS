using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class AddEmployeePage : Page
{
    public AddEmployeeViewModel ViewModel
    {
        get;
    }

    public AddEmployeePage()
    {
        ViewModel = App.GetService<AddEmployeeViewModel>();
        InitializeComponent();
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