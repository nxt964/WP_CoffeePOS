using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CoffeePOS.Converters;

public class StatisticsOrderStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string status)
            return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)); // Gray

        return status.ToLower() switch
        {
            "completed" => new SolidColorBrush(Color.FromArgb(255, 16, 124, 16)), // Green
            "pending" => new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)), // Blue
            "cancelled" => new SolidColorBrush(Color.FromArgb(255, 232, 17, 35)), // Red
            "processing" => new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)), // Orange
            _ => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)) // Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StatisticsEmptyCollectionVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count && count == 0)
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}




/// <summary>
/// Formats a number as a percentage with 2 decimal places
/// </summary>
public class PercentageFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double percentage)
        {
            return $"{percentage:P2}";
        }
        return "0.00%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Formats an integer count as "X orders"
/// </summary>
public class OrderCountFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return $"{count} orders";
        }
        return "0 orders";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Formats an integer count as "X purchase transactions"
/// </summary>
public class TransactionCountFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return $"{count} purchase transactions";
        }
        return "0 purchase transactions";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Formats a double as a percentage followed by "margin"
/// </summary>
public class ProfitMarginFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double margin)
        {
            return $"{margin:P2} margin";
        }
        return "0.00% margin";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Formats an integer as "X items sold"
/// </summary>
public class ItemsSoldFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return $"{count} items sold";
        }
        return "0 items sold";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}