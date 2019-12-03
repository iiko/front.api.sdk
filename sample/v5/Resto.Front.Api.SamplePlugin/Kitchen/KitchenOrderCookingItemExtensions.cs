using System;
using Resto.Front.Api.V5.Attributes.JetBrains;
using Resto.Front.Api.V5.Data.Kitchen;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    public static class KitchenOrderCookingItemExtensions
    {
        public static string GetCookingItemName([NotNull] this IKitchenOrderCookingItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            var sizeName = item.Size != null ? item.Size.Name : string.Empty;

            var product = item as IKitchenOrderItemProduct;
            if (product != null)
                return string.Format("{0} {1}", product.Product.Name, sizeName);

            var compound = item as IKitchenOrderCompoundItem;
            if (compound != null)
            {
                return compound.SecondaryComponent == null
                    ? string.Format("{0} {1}", compound.PrimaryComponent.Product.Name, sizeName)
                    : string.Format("{0} {1}", compound.Template.Name, sizeName);
            }

            throw new NotSupportedException();
        }

        public static decimal GetCookingItemAmount([NotNull] this IKitchenOrderCookingItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            var product = item as IKitchenOrderItemProduct;
            if (product != null)
                return product.Amount;

            var compound = item as IKitchenOrderCompoundItem;
            if (compound != null)
            {
                return compound.SecondaryComponent == null
                    ? compound.PrimaryComponent.Amount
                    : compound.PrimaryComponent.Amount + compound.PrimaryComponent.Amount;
            }

            throw new NotSupportedException();
        }
    }
}
