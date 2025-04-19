using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace CoffeePOS.Converters;

public class BoolToStockBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isStocked)
        {
            return isStocked
                ? new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)) // Green
                : new SolidColorBrush(Color.FromArgb(255, 231, 76, 60)); // Red
        }
        return new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)); // Gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ProductDetailCountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count && parameter is string mode)
        {
            if (mode == "Empty")
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            else if (mode == "NotEmpty")
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StockIndicatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int quantity && parameter is int threshold)
        {
            if (quantity <= 0)
                return new SolidColorBrush(Color.FromArgb(255, 231, 76, 60)); // Red
            else if (quantity <= threshold)
                return new SolidColorBrush(Color.FromArgb(255, 243, 156, 18)); // Yellow/Orange
            else
                return new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)); // Green
        }
        return new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)); // Gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}