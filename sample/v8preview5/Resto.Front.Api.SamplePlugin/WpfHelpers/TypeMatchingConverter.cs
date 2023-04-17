using System;
using System.Globalization;
using System.Windows.Data;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class TypeMatchingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            var typeToMatch = (Type)parameter;
            return typeToMatch.IsInstanceOfType(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
