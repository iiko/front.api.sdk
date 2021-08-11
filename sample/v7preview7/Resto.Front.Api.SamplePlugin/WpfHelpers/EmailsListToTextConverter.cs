using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Resto.Front.Api.Data.Brd;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal sealed class EmailsListToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;

            var emails = (ICollection<IEmail>)value;
            return string.Join(Environment.NewLine, emails.Select(email => email.Value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
