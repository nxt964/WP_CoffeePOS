using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Globalization;
using Windows.UI;

namespace CoffeePOS.Converters;

// This class is used to convert boolean values to a corresponding color
public class StatisticsBoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isInverse = false;
        if (parameter != null && parameter.ToString() == "inverse")
        {
            isInverse = true;
        }

        // Default is positive values are green (like revenue up), negative values are red
        if (value is decimal decimalValue)
        {
            bool isPositive = decimalValue >= 0;
            // For costs, we invert the logic if specified
            if (isInverse)
            {
                isPositive = !isPositive;
            }

            return isPositive
                ? new SolidColorBrush(Color.FromArgb(255, 0, 150, 0))  // Green
                : new SolidColorBrush(Color.FromArgb(255, 190, 0, 0)); // Red
        }

        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)); // Gray for undefined
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


// Converts boolean stock status to appropriate background color
public class StatisticsBoolToStockBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLowStock)
        {
            return isLowStock
                ? new SolidColorBrush(Color.FromArgb(255, 190, 0, 0))  // Red for low stock
                : new SolidColorBrush(Color.FromArgb(255, 0, 150, 0)); // Green for normal stock
        }

        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)); // Gray for undefined
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converts stock status to text indicator
public class StatisticsStockIndicatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLowStock)
        {
            return isLowStock ? "LOW STOCK" : "NORMAL";
        }

        return "UNKNOWN";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converts transaction type to appropriate background color
public class StatisticsTransactionTypeBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string transactionType)
        {
            return transactionType.ToUpper() == "IMPORT"
                ? new SolidColorBrush(Color.FromArgb(255, 0, 120, 215))  // Blue for imports
                : new SolidColorBrush(Color.FromArgb(255, 190, 0, 0));   // Red for exports
        }

        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)); // Gray for undefined
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converts order status to appropriate color
public class StatisticsOrderStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status)
        {
            return status.ToUpper() switch
            {
                "COMPLETED" => new SolidColorBrush(Color.FromArgb(255, 0, 150, 0)),    // Green
                "PENDING" => new SolidColorBrush(Color.FromArgb(255, 230, 150, 0)),    // Orange
                "CANCELLED" => new SolidColorBrush(Color.FromArgb(255, 190, 0, 0)),    // Red
                _ => new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),          // Gray for other statuses
            };
        }

        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)); // Gray for undefined
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Formats currency values with proper symbol
public class StatisticsCurrencyFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal amount)
        {
            return $"${amount:N2}";
        }

        if (value is double dAmount)
        {
            return $"${dAmount:N2}";
        }

        if (value is int iAmount)
        {
            return $"${iAmount:N2}";
        }

        return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Formats numeric values with proper decimals
public class StatisticsPriceFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string suffix = parameter as string ?? "";

        if (value is decimal amount)
        {
            return $"{amount:N2}{suffix}";
        }

        if (value is double dAmount)
        {
            return $"{dAmount:N2}{suffix}";
        }

        if (value is int iAmount)
        {
            return $"{iAmount}{suffix}";
        }

        return $"0{suffix}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


public class StringFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return string.Empty;

        if (parameter == null)
            return value.ToString();

        string format = parameter.ToString();

        // Dành cho định dạng ngày tháng
        if (value is DateTime dateTime)
        {
            return dateTime.ToString(format);
        }

        // Dành cho định dạng số
        if (value is int || value is double || value is decimal)
        {
            return string.Format(CultureInfo.CurrentCulture, format, value);
        }

        // Định dạng thông thường với string.Format
        return string.Format(format, value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // Thường không cần ConvertBack cho định dạng hiển thị
        throw new NotImplementedException();
    }
}



public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            return new DateTimeOffset(dateTime);
        }
        return DateTimeOffset.Now;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.DateTime;
        }
        return DateTime.Now;
    }
}
