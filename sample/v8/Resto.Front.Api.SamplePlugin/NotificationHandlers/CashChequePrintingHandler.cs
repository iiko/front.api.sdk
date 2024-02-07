using System;
using System.Runtime.Remoting;
using System.Xml.Linq;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.Orders;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public sealed class CashChequePrintingHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public CashChequePrintingHandler()
        {
            subscription = PluginContext.Notifications.CashChequePrinting.Subscribe(OnCashChequePrinting);
        }

        private static CashCheque OnCashChequePrinting(Guid orderId)
        {
            PluginContext.Log.Info("On cash cheque printing subscription.");

            var order = PluginContext.Operations.GetOrderById(orderId);
            var message = order.Status == OrderStatus.Closed
                ? $"Order #{order.Number} storno."
                : $"Order #{order.Number} pay.";
            
            return new CashCheque
            {
                BeforeCheque = new XElement(Tags.Center, message),
                AfterCheque = new XElement(Tags.QRCode, message)
            };
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
