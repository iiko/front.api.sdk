using System;
using System.Linq;
using System.Runtime.Remoting;
using Resto.Front.Api.Data.Orders;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    internal class PastOrderStornedHandler : IDisposable
    {
        private readonly IDisposable subscription;
        public PastOrderStornedHandler()
        {
            subscription = PluginContext.Notifications.PastOrderStorned.Subscribe(OnPastOrderStorned);
        }

        private void OnPastOrderStorned(StornedPastOrderInfo stornedPastOrderInfo)
        {
            // If OrderId == Guid.Empty and Number == 0 then set of past items have been storned instead of past order.
            if (stornedPastOrderInfo.PastOrderInfo == null)
            {
                MessageBox.Show($"Past items with total sum {stornedPastOrderInfo.Items.Sum(i => i.SumWithDiscounts)} have been storned " +
                                $"by order {stornedPastOrderInfo.StornoOrderInfo.OrderId} with number {stornedPastOrderInfo.StornoOrderInfo.OrderNum}.");
            }
            else
            {
                MessageBox.Show($"Past order {stornedPastOrderInfo.PastOrderInfo.OrderId} with number {stornedPastOrderInfo.PastOrderInfo.OrderNum} has been storned " +
                                $"by order {stornedPastOrderInfo.StornoOrderInfo.OrderId} with number {stornedPastOrderInfo.StornoOrderInfo.OrderNum}.");
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
