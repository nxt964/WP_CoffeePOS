using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CoffeePOS.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value as string))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] StringToVisibilityConverter.Convert: {ex.Message}");
            return Visibility.Collapsed; // Trả về giá trị mặc định nếu có lỗi
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}