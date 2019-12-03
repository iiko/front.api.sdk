using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;

namespace Resto.Front.Api.SamplePlugin.DiagnosticMessagesTester
{
    internal sealed class OrderItemReadyChangeNotifier : IDisposable
    {
        private readonly IDisposable subscription;

        public OrderItemReadyChangeNotifier()
        {
            var ordersItems = PluginContext.Operations
                                .GetOrders()
                                .Where(o => !(o is IDeliveryOrder) && o.Status == OrderStatus.New)
                                .ToDictionary(order => order.Id, order => GetProductsCookedByOrderSafe(order).ToList());

            subscription = PluginContext.Notifications
                .OrderChanged
                .Select(e => e.Entity)
                .Where(o => o.Status == OrderStatus.New)
                .Select(o => new
                    {
                        OrderId = o.Id,
                        CookedItems = GetProductsCookedByOrderSafe(o).ToArray()
                    })
                .Select(currentOrdersItems => new
                    {
                        currentOrdersItems.OrderId,
                        currentOrdersItems.CookedItems,
                        NewCookedItems = currentOrdersItems.CookedItems.Length > 0 && ordersItems.ContainsKey(currentOrdersItems.OrderId)
                                                      ? currentOrdersItems.CookedItems.Except(ordersItems[currentOrdersItems.OrderId]).ToArray()
                                                      : currentOrdersItems.CookedItems
                    })
                .Do(changes => ordersItems[changes.OrderId] = changes.CookedItems.ToList())
                .Where(changes => changes.NewCookedItems.Length > 0)
                .Subscribe(changes => OrderItemCooked(changes.NewCookedItems));

            PluginContext.Log.Info("OrderItemReadyChangeNotifier started.");
        }

        private void OrderItemCooked(IEnumerable<IOrderCookingItem> products)
        {
            var message = new StringBuilder();
            message.AppendLine("Ready products:");
            message.AppendLine();
            foreach (var readyOrderItem in products)
            {
                message.AppendLine(GetCookingItemName(readyOrderItem));
            }
            var output = message.ToString();
            MessageBox.Show(output);
            PluginContext.Log.Info(output);
        }

        public void Dispose()
        {
            subscription.Dispose();
        }

        private static IEnumerable<IOrderCookingItem> GetProductsCookedByOrderSafe(IOrder order)
        {
            return order.Items.OfType<IOrderCookingItem>().Where(x => x.Status == OrderItemStatus.CookingCompleted);
        }

        [NotNull, Pure]
        private string GetCookingItemName([NotNull] IOrderCookingItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var productItem = item as IOrderProductItem;
            if (productItem != null)
                return productItem.Product.Name;

            var compoundItem = (IOrderCompoundItem)item;
            return compoundItem.SecondaryComponent == null
                ? compoundItem.PrimaryComponent.Product.Name
                : string.Format("{0}/{1}", compoundItem.PrimaryComponent.Product.Name, compoundItem.SecondaryComponent.Product.Name);
        }
    }
}
