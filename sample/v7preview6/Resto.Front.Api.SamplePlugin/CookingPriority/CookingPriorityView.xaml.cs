using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin.CookingPriority
{
    public sealed partial class CookingPriorityView
    {
        private readonly ObservableCollection<IOrder> orders = new ObservableCollection<IOrder>();

        public ObservableCollection<IOrder> Orders
        {
            get { return orders; }
        }

        public CookingPriorityView()
        {
            InitializeComponent();
            ReloadOrders();
        }

        internal void ReloadOrders()
        {
            orders.Clear();
            PluginContext.Operations.GetOrders()
                .Where(order => order.Status == OrderStatus.New || order.Status == OrderStatus.Bill)
                .Where(order => order.Items.Any(item => !item.Deleted && item.Status != OrderItemStatus.Added))
                .ForEach(orders.Add);
        }

        private void RaiseCookingPriority(object sender, RoutedEventArgs e)
        {
            var order = GetOrderFromEventSender(sender);
            ChangeCookingPriority(order, order.CookingPriority + 1, order.IsTopCookingPriority);
        }

        private void ReduceCookingPriority(object sender, RoutedEventArgs e)
        {
            var order = GetOrderFromEventSender(sender);
            ChangeCookingPriority(order, order.CookingPriority - 1, order.IsTopCookingPriority);
        }

        private void SwitchTopCookingPriority(object sender, RoutedEventArgs e)
        {
            var order = GetOrderFromEventSender(sender);
            ChangeCookingPriority(order, order.CookingPriority, !order.IsTopCookingPriority);
        }

        private void ChangeCookingPriority(IOrder order, int cookingPriority, bool isTopCookingPriority)
        {
            PluginContext.Log.InfoFormat("Changing cooking priority of order {0} to {1} ({2})", order.Number, cookingPriority, isTopCookingPriority ? "vip" : "regular");
            PluginContext.Operations.ChangeCookingPriority(cookingPriority, isTopCookingPriority, order, PluginContext.Operations.GetCredentials());

            // NOTE: performance warning
            // Do not reload all orders every time in a real production code, only replace single changed order.
            // You may get it from ISubmittedEntities by using explicit edit session.
            ReloadOrders();
        }

        private static IOrder GetOrderFromEventSender(object sender)
        {
            return (IOrder)((FrameworkElement)sender).DataContext;
        }
    }
}
