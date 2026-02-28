using System;
using System.Reactive.Disposables;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Screens;
using Resto.Front.Api.Data.Security;

namespace Resto.Front.Api.SamplePlugin.DiagnosticMessagesTester
{
    internal sealed class FrontScreenReminder : IDisposable
    {
        private readonly CompositeDisposable subscriptions = new CompositeDisposable();
        private readonly string unicodeString = char.ConvertFromUtf32(0x1F60A);
        [CanBeNull]
        private IUser currentLoggedUser;

        public FrontScreenReminder()
        {
            subscriptions.Add(PluginContext.Notifications.CurrentUserChanged.Subscribe(user => currentLoggedUser = user));
            subscriptions.Add(PluginContext.Notifications.ScreenChanged
                .Subscribe(screen =>
                {
                    var message = string.Empty;

                    if (screen is IAdditionalOperationsScreen)
                    {
                        if (currentLoggedUser != null)
                            message = $"{currentLoggedUser.Name}, you should smile more often! {unicodeString}";
                    }
                    else if (screen is IOrderEditScreen orderEditScreen)
                    {
                        var order = orderEditScreen.Order;
                        message = $"You are on the order #{order.Number} edit screen.";
                    }
                    else if (screen is IDeliveryOrderEditScreen deliveryOrderEditScreen)
                    {
                        var order = deliveryOrderEditScreen.Order;
                        message = $"You are on the delivery order #{order.Number} edit screen. Client's name: {order.Client?.Name ?? order.ClientName}.";
                    }
                    else if (screen is IReserveEditScreen reserveEditScreen)
                    {
                        var reserve = reserveEditScreen.Reserve;
                        message = $"You are on the reserve/banquet edit screen. Client's name: {reserve.Client.Name}.";
                    }
                    else if (screen is IClosedOrderScreen closedOrderScreen)
                    {
                        var order = closedOrderScreen.Order;
                        message = $"You are on the closed order #{order.Number} screen.";
                    }
                    else if (screen is IUnmodifiableDeliveryOrderScreen unmodifiableDeliveryOrderScreen)
                    {
                        var order = unmodifiableDeliveryOrderScreen.Order;
                        message = $"You are on the unmodifiable delivery order #{order.Number} screen. Client's name: {order.Client?.Name ?? order.ClientName}.";
                    }
                    else if (screen is IOrderPayScreen orderPayScreen)
                    {
                        var order = orderPayScreen.Order;
                        message = $"You are on the order #{order.Number} payment screen. Result sum: {order.ResultSum}.";
                    }
                    else if (screen is IOpenCafeSessionScreen openCafeSessionScreen)
                    {
                        var cashRegister = openCafeSessionScreen.CashRegister;
                        message = $"You are on the cafe session opening wizard. Cash register: “{cashRegister.FriendlyName}”.";
                    }
                    else if (screen is ICloseCafeSessionScreen closeCafeSessionScreen)
                    {
                        var cashRegister = closeCafeSessionScreen.CashRegister;
                        var cafeSession = PluginContext.Operations.TryGetCafeSessionByCashRegister(cashRegister);
                        if (cafeSession != null)
                            message = $"You are on the cafe session #{cafeSession.Number} closing wizard. Cash register: “{cashRegister.FriendlyName}”.";
                    }
                    else if (screen is IOpenOrdersScreen
                             or IClosedOrdersScreen
                             or IPreliminaryOrdersScreen
                             or IDocumentsScreen
                             or ISectionSchemaScreen
                             or IOrdersByTablesScreen
                             or IOrdersByWaiterScreen
                             or ITabsByWaiterScreen
                             or IDeliveriesScreen
                             or IReservesScreen)
                    {
                        if (currentLoggedUser != null)
                            message = $"This is {screen.GetType().Name}.";
                    }

                    if (!string.IsNullOrWhiteSpace(message))
                        PluginContext.Operations.AddNotificationMessage(message, "SamplePlugin", TimeSpan.FromSeconds(7));
                }));
        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }
    }
}
