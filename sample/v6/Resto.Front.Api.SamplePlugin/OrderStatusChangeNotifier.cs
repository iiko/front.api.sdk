using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Remoting;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class OrderStatusChangeNotifier : IDisposable
    {
        #region Internal logic
        private readonly Stack<IDisposable> subscriptions = new Stack<IDisposable>();
        [NotNull]
        private readonly IHostTerminal terminal;

        public OrderStatusChangeNotifier()
        {
            terminal = PluginContext.Operations.GetHostTerminal();
            var orders = PluginContext.Operations.GetOrders();
            // NOTE тут пустой список, т.к. удаленные заказы не выставляются в api
            var deletedAndStornedOrderIds = orders
                .Where(o => o.Status == OrderStatus.Deleted)
                .Select(o => o.Id)
                .ToList();
            // удаление пустых заказов возможно и на некассовом терминале
            subscriptions.Push(
                PluginContext.Notifications.SubscribeOnBeforeDeleteOrder((o, p) =>
                {
                    deletedAndStornedOrderIds.Add(o.Id);
                    OnOrderDeleting(o);
                }));

            var pointsOfSale = PluginContext.Operations.GetHostTerminalPointsOfSale();
            var pointsOfSaleId = pointsOfSale.Select(i => i.Id).ToArray();
            if (pointsOfSaleId.Length == 0)
            {
                PluginContext.Log.Info("It's not cash terminal, orders will not be closed on this terminal.");
                return;
            }

            var closedOrderIds = orders
                .Where(o => o.Status == OrderStatus.Closed)
                .Select(o => o.Id)
                .ToList();
            PluginContext.Log.InfoFormat("On init: closed orders '{0}', deleted orders '{1}'.", closedOrderIds.Count,
                deletedAndStornedOrderIds.Count);
            subscriptions.Push(
                PluginContext.Notifications.OrderChanged
                    .Select(e => e.Entity)
                    .Where(o => o.Status == OrderStatus.Closed
                                && !closedOrderIds.Contains(o.Id) 
                                && !deletedAndStornedOrderIds.Contains(o.Id))
                    .Do(o => closedOrderIds.Add(o.Id))
                    .Where(IsSavedOnCurrentTerminal)
                    .Subscribe(OnOrderClosed));
            subscriptions.Push(
                PluginContext.Notifications.OrderChanged
                    .Select(e => e.Entity)
                    .Where(o => o.Status == OrderStatus.Deleted && !deletedAndStornedOrderIds.Contains(o.Id))
                    .Do(o => deletedAndStornedOrderIds.Add(o.Id))
                    .Where(IsStornedOnCurrentTerminal)
                    .Subscribe(OnOrderStorned));
        }

        private bool IsSavedOnCurrentTerminal(IOrder order)
        {
            var orderTerminal = PluginContext.Operations.GetLastChangedOrderTerminal(order);
            return terminal.Equals(orderTerminal);
        }

        private bool IsStornedOnCurrentTerminal(IOrder order)
        {
            // Таким экзотическим методом дополнительно фильтруем удалённые заказы от возвращаемых.
            if (order.Items.All(i => i.DeletionMethod != null))
                return false;
            return IsSavedOnCurrentTerminal(order);
        }

        public void Dispose()
        {
            while (subscriptions.Any())
            {
                var subscription = subscriptions.Pop();
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
        #endregion

        private void OnOrderClosed(IOrder order)
        {
            PluginContext.Log.Info($"Order {order.Number} ({order.Id}) is closed.");
        }

        private void OnOrderStorned(IOrder order)
        {
            PluginContext.Log.Info($"Order {order.Number} ({order.Id}) is storned.");
        }

        private void OnOrderDeleting(IOrder order)
        {
            PluginContext.Log.Info($"Order {order.Number} ({order.Id}) is deleting.");
        }
    }
}
