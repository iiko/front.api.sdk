using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Resto.Front.Api.CustomerScreen.Converters
{
    [ValueConversion(typeof(decimal), typeof(Visibility))]
    public sealed class NotDecimalZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            return ((decimal)value) == 0m ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
