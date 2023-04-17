using Resto.Front.Api.Data.Orders;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class OrderProductItemViewModel : OrderItemViewModel
    {
        private readonly IOrderProductItem orderProductItem;

        public override string DisplayText => orderProductItem.ProductCustomName ?? orderProductItem.Product.Name;
        public override decimal Price => orderProductItem.Price;
        public override decimal Amount => orderProductItem.Amount;

        public OrderProductItemViewModel(IOrderProductItem orderProductItem)
        {
            this.orderProductItem = orderProductItem;
        }
    }
}
