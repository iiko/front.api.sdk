using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    class Base64StringToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string base64String = (string) value;

            var image = new BitmapImage();
            if (base64String != null)
            {
                var stream = new MemoryStream(System.Convert.FromBase64String(base64String));
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
