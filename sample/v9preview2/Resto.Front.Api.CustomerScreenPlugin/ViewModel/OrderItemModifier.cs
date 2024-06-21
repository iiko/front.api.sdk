using System.Windows;
using Resto.Front.Api.Data.Orders;
using DpHelper = Resto.Front.Api.CustomerScreen.Helpers.DependencyPropertyHelper<Resto.Front.Api.CustomerScreen.ViewModel.OrderItemModifier>;

namespace Resto.Front.Api.CustomerScreen.ViewModel
{
    internal sealed class OrderItemModifier : OrderItem
    {
        private static readonly DependencyProperty AmountProperty = DpHelper.Register(o => o.Amount);
        private static readonly DependencyProperty PriceProperty = DpHelper.Register(o => o.Price);
        private static readonly DependencyProperty SumProperty = DpHelper.Register(o => o.Sum);

        private string Amount
        {
            get => (string)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }

        private decimal Price
        {
            get => (decimal)GetValue(PriceProperty);
            set => SetValue(PriceProperty, value);
        }

        public OrderItemModifier(IOrderModifierItem source, string amountString)
            : base(source, source.Product.Name)
        {
            Amount = amountString;
            Price = source.Price;
            Sum = source.Price * source.Amount;
        }

        private decimal Sum
        {
            get => (decimal)GetValue(SumProperty);
            set => SetValue(SumProperty, value);
        }

        public OrderItemModifier(IFixedChildModifier source)
            : base(null, source.Product.Name)
        {
            Amount = "0";
            Price = source.Product.Price;
        }
    }
}
