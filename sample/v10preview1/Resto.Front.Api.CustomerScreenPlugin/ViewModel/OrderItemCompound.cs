using System;
using System.Globalization;
using System.Windows;
using Resto.Front.Api.Data.Orders;
using DpHelper = Resto.Front.Api.CustomerScreen.Helpers.DependencyPropertyHelper<Resto.Front.Api.CustomerScreen.ViewModel.OrderItemCompound>;

namespace Resto.Front.Api.CustomerScreen.ViewModel
{
    internal sealed class OrderItemCompound : OrderItem
    {
        private static readonly DependencyProperty IntegerAmountPartProperty = DpHelper.Register(o => o.IntegerAmountPart);
        private static readonly DependencyProperty FractionalAmountPartProperty = DpHelper.Register(o => o.FractionalAmountPart);
        private static readonly DependencyProperty PriceProperty = DpHelper.Register(o => o.Price);

        public OrderItemCompound(IOrderCompoundItem source)
            : base(source)
        {
            IntegerAmountPart = GetFormattedIntegerAmountPart(source.Amount);
            FractionalAmountPart = GetFormattedFractionalAmountPart(source.Amount);
            Price = source.PrimaryComponent.Product.Price;
        }

        private string IntegerAmountPart
        {
            get => (string)GetValue(IntegerAmountPartProperty);
            set => SetValue(IntegerAmountPartProperty, value);
        }

        private string FractionalAmountPart
        {
            get => (string)GetValue(FractionalAmountPartProperty);
            set => SetValue(FractionalAmountPartProperty, value);
        }

        private decimal Price
        {
            get => (decimal)GetValue(PriceProperty);
            set => SetValue(PriceProperty, value);
        }

        private static string GetFormattedIntegerAmountPart(decimal amount)
        {
            return ((int)amount).ToString(NumberFormatInfo.CurrentInfo);
        }

        private static string GetFormattedFractionalAmountPart(decimal amount)
        {
            amount = Math.Round(amount, 3, MidpointRounding.AwayFromZero);
            var fract = amount - Math.Floor(amount);
            if (fract <= 0)
                return string.Empty;

            var fractPartLength = 0;
            while (fract != Math.Floor(fract))
            {
                fract *= 10;
                fractPartLength++;
            }

            var formatString = $"{{0}}{{1:D{fractPartLength}}}";
            return string.Format(formatString, NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, (int)fract);
        }
    }
}
