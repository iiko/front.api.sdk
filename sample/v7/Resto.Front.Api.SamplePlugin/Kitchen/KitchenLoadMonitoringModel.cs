using System.Collections.Generic;
using Resto.Front.Api.Data.Organization.Sections;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    internal sealed class KitchenLoadMonitoringModel
    {
        public int OrdersCount { get; set; }

        public int ProductsCount { get; set; }

        public int IdleProductsCount { get; set; }

        public int ProcessingProductsCount { get; set; }
        
        public int ProcessingOverdueProductsCount { get; set; }

        public int ProcessedProductsCount { get; set; }

        public int ServedProductsCount { get; set; }

        public IDictionary<IRestaurantSection, int> KitchensLoad { get; set; }

        public IList<CookingProductModel> CookingProducts { get; set; }
    }
}