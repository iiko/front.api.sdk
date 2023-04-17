using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public sealed class BeforeDoChequeHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public BeforeDoChequeHandler()
        {
            subscription = PluginContext.Notifications.BeforeDoCheque.Subscribe(x => OnBeforeDoCheque(x.order, x.paymentItems, x.os, x.vm));
        }

        private static void OnBeforeDoCheque([NotNull] IOrder order, [NotNull] IReadOnlyCollection<IPaymentItem> paymentItems, [NotNull] IOperationService os, [CanBeNull] IViewManager vm)
        {
            PluginContext.Log.Info("On before do cheque subscription.");

            if (vm != null)
            {
                const string externalDataKey = "SamplePlugin_BeforeDoCheque";
                var externalData = os.TryGetOrderExternalDataByKey(order, externalDataKey);
                if (string.IsNullOrWhiteSpace(externalData))
                {
                    var add = vm.ShowOkCancelPopup(externalDataKey, $"Do you want to add external data to the order #{order.Number}?");
                    if (add)
                        os.AddOrderExternalData(externalDataKey, "some-external-data", false, order, os.GetCredentials());
                }
                else
                {
                    var delete = vm.ShowOkCancelPopup(externalDataKey, $"Found external data \"{externalData}\".{Environment.NewLine}Do you want to remove it?");
                    if (delete)
                        os.DeleteOrderExternalData(externalDataKey, order, os.GetCredentials());
                }
            }

            vm?.ChangeProgressBarMessage("Waiting for confirmation...");

            var message = $"Allow do cheque operation of the order #{order.Number}?";
            foreach (var payment in paymentItems)
                message += $"{Environment.NewLine}{payment.Type.Name} {payment.Type.Kind} {payment.Sum} {payment.Status}";

            if (!(vm?.ShowYesNoPopup("Sample", message) ?? true))
            {
                PluginContext.Log.Info($"Do cheque operation of order '{order.Id}' will be canceled.");
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
