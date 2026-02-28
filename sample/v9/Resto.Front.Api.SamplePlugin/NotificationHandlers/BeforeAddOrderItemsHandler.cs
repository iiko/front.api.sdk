using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public sealed class BeforeAddOrderItemsHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public BeforeAddOrderItemsHandler()
        {
            subscription = PluginContext.Notifications.BeforeAddOrderItems.Subscribe(x => OnBeforeAddOrderItemsHandler(x.order, x.addingItems, x.addingComponents, x.addingModifiers, x.user, x.vm));
        }

        private static void OnBeforeAddOrderItemsHandler(
            [NotNull] IOrder order,
            [NotNull] IReadOnlyCollection<IOrderRootItem> orderRootItems,
            [NotNull] IReadOnlyCollection<IOrderCompoundItemComponent> compoundItemComponents,
            [NotNull] IReadOnlyCollection<IOrderModifierItem> orderModifierItems,
            [NotNull] IUser user,
            [CanBeNull] IViewManager vm)
        {
            PluginContext.Log.Info($"Adding items to order #{order.Number} ({order.Id}).");

            var msg = $"Do you want to add items to the order #{order.Number}?";
            var title = "SamplePlugin";

            if (!(vm?.ShowYesNoPopup(title, msg) ??
                MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
            {
                PluginContext.Log.Info($"Adding items to order #{order.Number} ({order.Id}) canceled.");
                throw new OperationCanceledException();
            }
        }

        public void Dispose()
        {
            try
            {
                subscription.Dispose();
            }
            catch (RemotingException)
            {
                // nothing to do with the lost connection
            }
        }
    }
}
