using System;
using System.Linq;
using System.Runtime.Remoting;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.OperationContexts;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    internal sealed class NavigatingToPaymentScreenHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public NavigatingToPaymentScreenHandler()
        {
            subscription = PluginContext.Notifications.NavigatingToPaymentScreen.Subscribe(x => OnNavigatingToPaymentScreen(x.order, x.os, x.vm, x.context));
        }

        private void OnNavigatingToPaymentScreen([NotNull] IOrder order, [NotNull] IOperationService os, [NotNull] IViewManager vm, INavigatingToPaymentScreenOperationContext context)
        {
            PluginContext.Log.Info("On navigating to payment screen subscription.");

            var orderSection = order.Tables[0].RestaurantSection;
            if (orderSection == null)
                throw new OperationCanceledException($"Table {order.Tables[0].Name} is deleted, so there is no restaurant section.");
            var currentTableIndex = orderSection.Tables.ToList().IndexOf(order.Tables[0]);
            var selectedTableIndex = vm.ShowChooserPopup("Select new table before navigating to payment screen", orderSection.Tables.Select(u => u.Name).ToList(), currentTableIndex);
            if (selectedTableIndex >= 0)
            {
                var selectedTable = orderSection.Tables[selectedTableIndex];
                os.ChangeOrderTables(order, new[] { selectedTable }, os.GetCredentials());
            }

            vm.ChangeProgressBarMessage("Waiting for confirmation...");

            var message = $"Allow navigating to payment screen of the order #{order.Number}?";
            var allowOperation = vm.ShowYesNoPopup("Sample", message);

            if (!allowOperation)
            {
                PluginContext.Log.Info($"Navigating to payment screen of order '{order.Id}' will be canceled.");
                throw new OperationCanceledException();
            }

            var currentInfo = context.ChequeAdditionalInfo; // other plugins could set another values, try to respect them
            PluginContext.Log.Info($"{(currentInfo == null ? "Providing" : "Overriding")} cheque additional info...");

            context.ChequeAdditionalInfo = new ChequeAdditionalInfo(
                currentInfo?.NeedReceipt ?? true,
                currentInfo?.Phone,
                "mail@example.com",
                currentInfo?.SettlementPlace);
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
