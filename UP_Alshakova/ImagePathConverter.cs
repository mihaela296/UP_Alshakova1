using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace UP_Alshakova
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = value as string;

            if (string.IsNullOrEmpty(imagePath))
                return new BitmapImage(new Uri("Images/picture.png", UriKind.Relative));

            try
            {
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                return new BitmapImage(new Uri("Images/picture.png", UriKind.Relative));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}