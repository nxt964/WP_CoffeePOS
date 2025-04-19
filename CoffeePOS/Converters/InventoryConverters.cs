using CoffeePOS.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using Windows.UI;

namespace CoffeePOS.Converters;

// Modified LowStockVisibilityConverter
public class LowStockVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Handle the case where we're binding to the entire Ingredient
        if (value is Ingredient ingredient)
        {
            return ingredient.Quantity <= ingredient.Threshold ? Visibility.Visible : Visibility.Collapsed;
        }
        // Keep the original logic for backward compatibility
        else if (value is int quantity && parameter is int threshold)
        {
            return quantity <= threshold ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Modified NormalStockVisibilityConverter
public class NormalStockVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Handle the case where we're binding to the entire Ingredient
        if (value is Ingredient ingredient)
        {
            return ingredient.Quantity > ingredient.Threshold ? Visibility.Visible : Visibility.Collapsed;
        }
        // Keep the original logic for backward compatibility
        else if (value is int quantity && parameter is int threshold)
        {
            return quantity > threshold ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ObjectToVisibilityConverterInverted : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class EmptyCollectionVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StockBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int quantity && parameter is int threshold)
        {
            if (quantity <= 0)
                return new SolidColorBrush(Color.FromArgb(255, 224, 67, 67)); // Red
            else if (quantity <= threshold)
                return new SolidColorBrush(Color.FromArgb(255, 255, 159, 0)); // Orange
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

public class IngredientStockConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Ingredient ingredient)
        {
            int quantity = ingredient.Quantity;
            int threshold = ingredient.Threshold;
            Debug.WriteLine($"IngredientStockConverter - Quantity: {quantity}, Threshold: {threshold}");
            if (quantity <= 0)
                return new SolidColorBrush(Color.FromArgb(255, 224, 67, 67)); // Red
            else if (quantity <= threshold)
                return new SolidColorBrush(Color.FromArgb(255, 255, 159, 0)); // Orange
            else
                return new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)); // Green
        }
        return new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)); // Gray default
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class TransactionTypeBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string type)
        {
            return type.ToUpper() == "IMPORT"
                ? new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)) // Green
                : new SolidColorBrush(Color.FromArgb(255, 224, 67, 67)); // Red
        }
        return new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)); // Gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class TransactionTypeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string type)
        {
            return type.ToUpper() == "IMPORT"
                ? new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)) // Green
                : new SolidColorBrush(Color.FromArgb(255, 224, 67, 67)); // Red
        }
        return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)); // Black
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class UnixTimestampConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long timestamp)
        {
            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
            return dateTime.ToString("MMM dd, yyyy HH:mm");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class TransactionTotalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is IngredientInventoryTransaction transaction)
        {
            double total = transaction.Quantity * transaction.UnitPrice;
            return string.Format("${0:N2}", total);
        }
        return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}