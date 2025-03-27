using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CoffeePOS.Converters;
public class AbsolutePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return null;

        string filename = value.ToString();

        if (filename.StartsWith("C:\\Users"))
        {
            return new BitmapImage(new Uri(filename));
        }
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(folder, "Assets", filename);

        // Ensure the path is a valid URI
        if (File.Exists(path))
        {
            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }

        return new BitmapImage(new Uri("ms-appx:///Assets/ProductImageDefault.png")); // Provide a fallback image
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return "";
    }
}
