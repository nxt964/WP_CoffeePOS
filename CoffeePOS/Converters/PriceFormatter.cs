using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace CoffeePOS.Converters;
public class PriceFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double price)
        {
            return price.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
        }
        return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string priceString && double.TryParse(priceString, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"), out double price))
        {
            return price;
        }
        return 0.0;
    }
}
