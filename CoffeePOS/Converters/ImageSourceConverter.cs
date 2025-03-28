using System;
using Microsoft.UI.Xaml.Data;
using Windows.UI.Xaml.Data;

namespace CoffeePOS.Converters;

public class ImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string imageUrl && !string.IsNullOrEmpty(imageUrl))
        {
            try
            {
                return new Uri(imageUrl);
            }
            catch
            {
                // Trả về một hình ảnh mặc định nếu URL không hợp lệ
                return new Uri("ms-appx:///Assets/ProductImage.jpg");
            }
        }
        // Trả về hình ảnh mặc định nếu giá trị không hợp lệ
        return new Uri("ms-appx:///Assets/ProductImage.jpg");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}