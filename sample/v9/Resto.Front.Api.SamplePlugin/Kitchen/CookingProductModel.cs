using System;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Kitchen;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    internal sealed class CookingProductModel
    {
        public CookingProductModel([NotNull] IKitchenOrder order, [NotNull] IKitchenOrderCookingItem cookingItem)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            if (cookingItem == null)
                throw new ArgumentNullException(nameof(cookingItem));

            Order = order;
            CookingItem = cookingItem;
            Name = cookingItem.GetCookingItemName();
            Amount = cookingItem.GetCookingItemAmount();
        }

        public IKitchenOrder Order { get; }
        public IKitchenOrderCookingItem CookingItem { get; }
        public string Name { get; }
        public decimal Amount { get; }
    }
}