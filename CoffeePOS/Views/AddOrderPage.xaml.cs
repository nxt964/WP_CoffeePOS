using CoffeePOS.Core.Models;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace CoffeePOS.Views;

public sealed partial class AddOrderPage : Page
{
    public AddOrderViewModel ViewModel
    {
        get;
    }

    public AddOrderPage()
    {
        ViewModel = App.GetService<AddOrderViewModel>();
        InitializeComponent();
        ViewModel.SetXamlRoot(this.XamlRoot);
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderPage.AutoSuggestBox_TextChanged: Text = {sender.Text}");
            ViewModel.UpdateCustomerSuggestions(sender.Text);
        }
    }

    private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        var selectedCustomer = args.SelectedItem as Customer;
        if (selectedCustomer != null)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddOrderPage.AutoSuggestBox_SuggestionChosen: Selected Customer = {selectedCustomer.Name} (Id = {selectedCustomer.Id})");
            ViewModel.SelectedCustomer = selectedCustomer;
            ViewModel.CustomerName = selectedCustomer.Name;
            ViewModel.CustomerPhone = selectedCustomer.Phone; 
        }
    }

    private void CustomerPhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
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