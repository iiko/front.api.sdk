using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Brd;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal sealed class AddressesListToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;

            var addresses = (ICollection<IAddress>)value;
            return string.Join(Environment.NewLine, addresses.Select(ConvertAddressToText));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static string ConvertAddressToText([NotNull] IAddress address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            return string.Format("{0}, {1}, {2}", address.Street.Name, address.House, address.Flat);
        }
    }
}
