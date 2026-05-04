using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    [ValueConversion(typeof(bool[]), typeof(BitmapImage))]
    internal sealed class ImageBytesToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CreateBitmap((byte[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        [ContractAnnotation("null => null; notnull => notnull")]
        public static BitmapImage CreateBitmap([CanBeNull] byte[] source)
        {
            if (source == null)
                return null;

            using (var stream = new MemoryStream(source))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
    }
}
