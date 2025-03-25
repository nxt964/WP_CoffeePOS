using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace CoffeePOS.Converters
{
    public class CurrencyFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double price)
            {
                return price.ToString("C", CultureInfo.GetCultureInfo("en-US"));
            }
            return "$0.00"; // Default fallback value
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string priceString && double.TryParse(priceString, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"), out double price))
            {
                return price;
            }
            return 0.0; // Default fallback value
        }
    }
}
