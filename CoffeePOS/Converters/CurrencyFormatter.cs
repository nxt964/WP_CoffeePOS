using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace CoffeePOS.Converters
{
    public class CurrencyFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                // Định dạng thành tiền tệ dựa trên culture hiện tại
                return decimalValue.ToString("C", CultureInfo.CurrentCulture);
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}