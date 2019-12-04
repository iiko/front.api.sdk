using System;
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
            subscription = PluginContext.Notifications.SubscribeOnBeforeOrderBill(OnBeforeOrderBill);
        }

        private void OnBeforeOrderBill(IOrder order, [CanBeNull] IViewManager viewManager)
        {
            PluginContext.Log.Info("On before order bill subscription.");
            viewManager?.ChangeProgressBarMessage("Waiting for confirmation...");

            if (MessageBox.Show($"Allow bill operation of the order: #{order.Number}, guests {order.Guests.Count}, sum: {order.ResultSum}?{Environment.NewLine}", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
                != MessageBoxResult.Yes)
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
