using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Organization.Sections;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal sealed class ProductModel : MenuItemModel
    {
        public IProduct Product { get; }
        public decimal? RemainingAmount { get; }
        public bool IsSellingRestricted { get; }
        public string IncludedInMenuSections { get; }
        public Color Background => Product.BackgroundColor ?? Color.WhiteSmoke;
        public Color Foreground => Product.FontColor ?? Color.Black;

        public ProductModel([NotNull] IProduct product, [NotNull] IEnumerable<IRestaurantSection> includedInMenuSections, decimal? remainingAmount, bool isSellingRestricted)
        {
            IsSellingRestricted = isSellingRestricted;
            RemainingAmount = remainingAmount;
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (includedInMenuSections == null)
                throw new ArgumentNullException(nameof(includedInMenuSections));

            Product = product;
            IncludedInMenuSections = string.Join(", ", includedInMenuSections.Select(section => section.Name));
        }
    }
}
