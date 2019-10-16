using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal sealed class SystemColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var apiColor = (System.Drawing.Color)value;
            return new SolidColorBrush(Color.FromArgb(apiColor.A, apiColor.R, apiColor.G, apiColor.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}