using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal sealed class ApiColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Resto.Front.Api.V5.Data.View.Color apiColor = (Resto.Front.Api.V5.Data.View.Color)value;
            System.Windows.Media.Color color = new System.Windows.Media.Color
            {
                R = apiColor.R,
                G = apiColor.G,
                B = apiColor.B,
                A = apiColor.A
            };
            var brush = new SolidColorBrush(color);
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}