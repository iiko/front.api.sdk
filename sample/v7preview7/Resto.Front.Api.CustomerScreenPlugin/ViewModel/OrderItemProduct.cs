using System;
using System.Globalization;
using System.Windows;
using Resto.Front.Api.Data.Orders;
using DpHelper = Resto.Front.Api.CustomerScreen.Helpers.DependencyPropertyHelper<Resto.Front.Api.CustomerScreen.ViewModel.OrderItemProduct>;

namespace Resto.Front.Api.CustomerScreen.ViewModel
{
    internal sealed class OrderItemProduct : OrderItem
    {
        private static readonly DependencyProperty IntegerAmountPartProperty = DpHelper.Register(o => o.IntegerAmountPart);
        private static readonly DependencyProperty FractionalAmountPartProperty = DpHelper.Register(o => o.FractionalAmountPart);
        private static readonly DependencyProperty PriceProperty = DpHelper.Register(o => o.Price);

        public OrderItemProduct(IOrderProductItem source)
            : base(source, source.Product.Name)
        {
            IntegerAmountPart = GetFormattedIntegerAmountPart(source.Amount);
            FractionalAmountPart = GetFormattedFractionalAmountPart(source.Amount);
            Price = source.Price;
        }

        public string IntegerAmountPart
        {
            get { return (string)GetValue(IntegerAmountPartProperty); }
            set { SetValue(IntegerAmountPartProperty, value); }
        }

        public string FractionalAmountPart
        {
            get { return (string)GetValue(FractionalAmountPartProperty); }
            set { SetValue(FractionalAmountPartProperty, value); }
        }

        public decimal Price
        {
            get { return (decimal)GetValue(PriceProperty); }
            set { SetValue(PriceProperty, value); }
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

            var formatString = string.Format("{{0}}{{1:D{0}}}", fractPartLength);
            return string.Format(formatString, NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, (int)fract);
        }
    }
}
