using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI;
using Windows.UI;

namespace CoffeePOS.Converters;
internal class OrderStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status)
        {
            return status switch
            {
                "Pending" => new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),      
                "Complete" => new SolidColorBrush(Color.FromArgb(255, 0, 200, 0)),   
                "Cancel" => new SolidColorBrush(Color.FromArgb(255, 232, 17, 35)), 
                "In Progress" => new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)), 
                _ => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128))
            };
        }
        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
}
