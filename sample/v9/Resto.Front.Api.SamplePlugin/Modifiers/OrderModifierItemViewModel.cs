using Resto.Front.Api.Data.Orders;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class OrderModifierItemViewModel : OrderItemViewModel
    {
        private readonly IOrderModifierItem orderModifierItem;

        public override string DisplayText => orderModifierItem.ProductCustomName ?? orderModifierItem.Product.Name;
        public override decimal Price => orderModifierItem.Price;
        public override decimal Amount => orderModifierItem.Amount;

        public OrderModifierItemViewModel(IOrderModifierItem orderModifierItem)
        {
            this.orderModifierItem = orderModifierItem;
        }
    }
}
