using System;
using System.Runtime.Remoting;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public sealed class ConfirmEInvoiceNumberProcessedHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public ConfirmEInvoiceNumberProcessedHandler()
        {
            subscription = PluginContext.Notifications.ConfirmEInvoiceNumberProcessed.Subscribe(x => OnConfirmEInvoiceNumberProcessed(x.orderId, x.isEInvoiceNumberSuccessfullyProcessed));
        }

        private bool OnConfirmEInvoiceNumberProcessed(Guid orderId, bool isEInvoiceNumberSuccessfullyProcessed)
        {
            // Подтверждение успешного/неуспешного использования номера, выданного в GetEInvoiceNumberHandler
            PluginContext.Log.Info($"ConfirmEInvoiceNumberProcessedHandler. EInvoice Number Successfully Processed: {isEInvoiceNumberSuccessfullyProcessed}.");
            return true;
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
