using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal sealed class NotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;

            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
