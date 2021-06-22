using System.Windows;
using Resto.Front.Api.Data.Orders;
using DpHelper = Resto.Front.Api.CustomerScreen.Helpers.DependencyPropertyHelper<Resto.Front.Api.CustomerScreen.ViewModel.OrderItemModifier>;

namespace Resto.Front.Api.CustomerScreen.ViewModel
{
    internal sealed class OrderItemModifier : OrderItem
    {
        private static readonly DependencyProperty AmountProperty = DpHelper.Register(o => o.Amount);
        private static readonly DependencyProperty PriceProperty = DpHelper.Register(o => o.Price);

        public decimal Amount
        {
            get { return (decimal)GetValue(AmountProperty); }
            set { SetValue(AmountProperty, value); }
        }

        public decimal Price
        {
            get { return (decimal)GetValue(PriceProperty); }
            set { SetValue(PriceProperty, value); }
        }

        public OrderItemModifier(IOrderModifierItem source)
            : base(source, source.Product.Name)
        {
            Amount = source.Amount;
            Price = source.Price;
        }

        public OrderItemModifier(IFixedChildModifier source)
            : base(null, source.Product.Name)
        {
            Amount = 0;
            Price = source.Product.Price;
        }
    }
}
