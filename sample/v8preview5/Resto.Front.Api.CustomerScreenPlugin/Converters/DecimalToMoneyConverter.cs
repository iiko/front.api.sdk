using System;
using System.Globalization;
using System.Windows.Data;

namespace Resto.Front.Api.CustomerScreen.Converters
{
    [ValueConversion(typeof(decimal), typeof(string))]
    public sealed class DecimalToMoneyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var moneyFormat = parameter as MoneyDisplayFormat? ?? MoneyDisplayFormat.NumberAndCurrency; // формат по умолчанию

            var numberFormatInfo = CustomerScreenPlugin.CurrencySettings.FormatInfo;
            var format = !CustomerScreenPlugin.CurrencySettings.ShowFractionalPart ? "c0" : "c";
            var sum = MoneyRound(System.Convert.ToDecimal(value), numberFormatInfo.CurrencyDecimalDigits, CustomerScreenPlugin.CurrencySettings.MinimumDenomination);

            switch (moneyFormat)
            {
                case MoneyDisplayFormat.NumberAndCurrency:
                    {
                        if (string.IsNullOrEmpty(numberFormatInfo.CurrencySymbol))
                            numberFormatInfo.CurrencySymbol = CultureInfo.CurrentUICulture.NumberFormat.CurrencySymbol;
                        return sum.ToString(format, numberFormatInfo);
                    }
                case MoneyDisplayFormat.NumbersOnly:
                    {
                        return sum.ToString(numberFormatInfo);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Money format value not supported");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static decimal MoneyRound(decimal sum, int decimals, decimal minimumDenomination)
        {
            if (minimumDenomination < 0m)
                throw new ArgumentOutOfRangeException(nameof(minimumDenomination), minimumDenomination, "minimumDenomination must be nonnegative");

            var rounded = decimals >= 0
                ? Math.Round(sum, decimals, MidpointRounding.AwayFromZero)
                : Round(sum, (decimal)Math.Pow(10, -decimals));

            return minimumDenomination == 0m ? rounded : Round(rounded, minimumDenomination);
        }

        private static decimal Round(decimal value, decimal minimumDenomination)
        {
            if (value < 0m)
                return -Round(-value, minimumDenomination);

            var remainder = value % minimumDenomination;

            if ((minimumDenomination / 2m) > remainder)
                return value - remainder;

            return value - remainder + minimumDenomination;
        }
    }
}
