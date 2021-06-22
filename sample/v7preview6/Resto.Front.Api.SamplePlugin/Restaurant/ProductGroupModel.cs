using System;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal sealed class ProductGroupModel : MenuItemModel
    {
        public IProductGroup ProductGroup { get; }

        public ProductGroupModel([NotNull] IProductGroup productGroup)
        {
            if (productGroup == null)
                throw new ArgumentNullException(nameof(productGroup));

            ProductGroup = productGroup;
        }
    }
}
