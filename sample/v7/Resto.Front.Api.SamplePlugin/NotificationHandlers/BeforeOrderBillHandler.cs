using System;
using System.Linq;
using System.Runtime.Remoting;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public class BeforeOrderBillHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public BeforeOrderBillHandler()
        {
            subscription = PluginContext.Notifications.BeforeOrderBill.Subscribe(x => OnBeforeOrderBill(x.order, x.os, x.vm));
        }

        private void OnBeforeOrderBill([NotNull] IOrder order, [NotNull] IOperationService os, [CanBeNull] IViewManager vm)
        {
            if (order.Status != OrderStatus.New)
                return;

            PluginContext.Log.Info("On before order bill subscription.");

            if (vm != null)
            {
                var orderSection = order.Tables[0].RestaurantSection;
                if (orderSection == null)
                    throw new OperationCanceledException($"Table {order.Tables[0].Name} is deleted, so there is no restaurant section.");
                var currentTableIndex = orderSection.Tables.ToList().IndexOf(order.Tables[0]);
                var selectedTableIndex = vm.ShowChooserPopup("Select new table before order bill", orderSection.Tables.Select(u => u.Name).ToList(), currentTableIndex);
                if (selectedTableIndex >= 0)
                {
                    var selectedTable = orderSection.Tables[selectedTableIndex];
                    os.ChangeOrderTables(order, new[] {selectedTable}, os.GetCredentials());
                }
            }

            vm?.ChangeProgressBarMessage("Waiting for confirmation...");

            var message = $"Allow bill operation of the order #{order.Number}?";
            var allowOperation = vm?.ShowYesNoPopup("Sample", message) // prefer built-in popups
                ?? MessageBox.Show(message, "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes;

            if (!allowOperation)
            {
                PluginContext.Log.Info($"Bill operation of order '{order.Id}' will be canceled.");
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
