using System;
using System.Linq;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin.Screens
{
    public static class OrderEditScreen
    {
        public static IDisposable AddProductByBarcode()
        {
            return PluginContext.Notifications.OrderEditBarcodeScanned.Subscribe(x => HandleBarcodeScannedEvent(x.barcode, x.order, x.os, x.vm));
        }

        public static IDisposable AddDiscountByCard()
        {
            return PluginContext.Notifications.OrderEditCardSlided.Subscribe(x => HandleCardSlidedEvent(x.card, x.order, x.os));
        }

        private static bool HandleBarcodeScannedEvent(string barcode, IOrder order, IOperationService os, IViewManager vm)
        {
            // notify user about current operation to be more friendly
            // especially when operation may take longer than half a second
            vm.ChangeProgressBarMessage("Searching product by barcode...");

            var result = TryFindProductByBarcode(barcode);
            if (result == null)
                return false; // iikoFront will call another handler or finally show message that the barcode is unknown

            var (product, amount, price, name) = result.Value;

            // just an example of how to interact with the user within the barcode scanned event handler
            if (!vm.ShowOkCancelPopup("Sample Plugin", $"Do you want to add {product.Name}?"))
                return false;

            PluginContext.Log.Info($"Adding “{product.Name}” ({product.Id}) to order #{order.Number} ({order.Id}) with amount={amount}, price={price}, name=”{name}”");
            var es = os.CreateEditSession();
            var orderItem = es.AddOrderProductItem(amount, product, order, order.Guests.First(), null, predefinedPrice: price);
            es.SetProductItemCustomName(name, order, orderItem);
            os.SubmitChanges(os.GetCredentials(), es);

            return true;
        }

        private static (IProduct product, decimal amount, decimal price, string name)? TryFindProductByBarcode([NotNull] string barcode)
        {
            // implement your own barcode-to-product mapping
            // this example just gets first product
            var product = PluginContext.Operations
                .GetActiveProducts()
                .FirstOrDefault(p => p.Template == null && p.Scale == null && p.Type == ProductType.Goods);

            return product != null
                ? (product, 3m, 140m, $"Found by {barcode}")
                : ((IProduct, decimal, decimal, string)?)null;
        }

        private static bool HandleCardSlidedEvent(CardInputDialogResult card, IOrder order, IOperationService os)
        {
            // check some external loyalty system to for this card and determine appropriate discount or other actions
            var discountType = TryFindDiscountByCard(card.Track2);
            if (discountType == null)
                return false;

            os.AddDiscount(discountType, order, os.GetCredentials());
            return true;
        }

        [CanBeNull]
        private static IDiscountType TryFindDiscountByCard(string card)
        {
            if (card == "7736")
                return PluginContext.Operations.GetDiscountTypes().First(x => x.CanApplyManually);

            return null;
        }
    }
}
