using System;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Kitchen;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    public static class KitchenOrderCookingItemExtensions
    {
        public static string GetCookingItemName([NotNull] this IKitchenOrderCookingItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var sizeName = item.Size != null ? item.Size.Name : string.Empty;
            switch (item)
            {
                case IKitchenOrderItemProduct product:
                    return $"{product.Product.Name} {sizeName}";
                case IKitchenOrderCompoundItem compound:
                    return compound.SecondaryComponent == null
                        ? $"{compound.PrimaryComponent.Product.Name} {sizeName}"
                        : $"{compound.Template.Name} {sizeName}";
                default:
                    throw new NotSupportedException();
            }
        }

        public static decimal GetCookingItemAmount([NotNull] this IKitchenOrderCookingItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            switch (item)
            {
                case IKitchenOrderItemProduct product:
                    return product.Amount;
                case IKitchenOrderCompoundItem compound:
                    return compound.SecondaryComponent == null
                        ? compound.PrimaryComponent.Amount
                        : compound.PrimaryComponent.Amount + compound.PrimaryComponent.Amount;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
