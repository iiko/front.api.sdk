using System;
using System.Globalization;
using System.Windows.Data;

namespace Resto.Front.Api.CustomerScreen.Converters
{
    [ValueConversion(typeof(decimal), typeof(string))]
    public sealed class DecimalHundredToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(CultureInfo.CurrentUICulture, "{0:0.00}" + CultureInfo.CurrentUICulture.NumberFormat.PercentSymbol, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return decimal.Parse(((string)value).Replace(CultureInfo.CurrentUICulture.NumberFormat.PercentSymbol, string.Empty), CultureInfo.CurrentUICulture);
        }
    }
}
