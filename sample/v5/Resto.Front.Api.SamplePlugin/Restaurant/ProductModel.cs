using System;
using System.Collections.Generic;
using System.Linq;
using Resto.Front.Api.V5.Attributes.JetBrains;
using Resto.Front.Api.V5.Data.Assortment;
using Resto.Front.Api.V5.Data.Organization.Sections;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal sealed class ProductModel : MenuItemModel
    {
        public IProduct Product { get; private set; }
        public decimal? RemainingAmount { get; private set; }
        public bool IsSellingRestricted { get; private set; }
        public string IncludedInMenuSections { get; private set; }

        public ProductModel([NotNull] IProduct product, [NotNull] IEnumerable<IRestaurantSection> includedInMenuSections, decimal? remainingAmount, bool isSellingRestricted)
        {
            IsSellingRestricted = isSellingRestricted;
            RemainingAmount = remainingAmount;
            if (product == null)
                throw new ArgumentNullException("product");
            if (includedInMenuSections == null)
                throw new ArgumentNullException("includedInMenuSections");

            Product = product;
            IncludedInMenuSections = string.Join(", ", includedInMenuSections.Select(section => section.Name));
        }
    }
}
