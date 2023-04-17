using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.PreliminaryOrders;
using Resto.Front.Api.Exceptions;

namespace Resto.Front.Api.SamplePlugin.PreliminaryOrders
{
    internal sealed partial class OrdersView : IDisposable
    {
        #region Fields
        private readonly ObservableCollection<OrderModel> orderModels = new ObservableCollection<OrderModel>();

        private readonly IDisposable subscription;
        #endregion

        #region Ctor & Dispose
        public OrdersView()
        {
            InitializeComponent();

            DataContext = orderModels;

            subscription = PluginContext.Notifications.PreliminaryOrderChanged
                .ObserveOnDispatcher()
                .Subscribe(UpdateOrder);

            foreach (var order in PluginContext.Operations.GetPreliminaryOrders().OrderBy(order => order.CreateTime))
            {
                var model = new OrderModel();
                model.Update(order);
                orderModels.Add(model);
            }
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
        #endregion

        #region Methods
        private void UpdateOrder(EntityChangedEventArgs<IPreliminaryOrder> args)
        {
            var order = args.Entity;
            var model = orderModels.FirstOrDefault(om => om.Id == order.Id);

            if (args.EventType == EntityEventType.Removed)
            {
                orderModels.Remove(model);
            }
            else
            {
                if (model == null)
                {
                    model = new OrderModel();
                    model.Update(order);
                    orderModels.Add(model);
                }
                else
                {
                    model.Update(order);
                }
            }
        }

        private bool ShowEditor(OrderModel model)
        {
            var window = new Window
                         {
                             MinHeight = 480,
                             Height = 480,
                             MinWidth = 640,
                             Width = 640,
                             SizeToContent = SizeToContent.Manual
                         };

            window.Content = new OrderEditView(window, model);

            return window.ShowDialog() == true;
        }

        private void ButtonCreateClick(object sender, RoutedEventArgs args)
        {
            var newOrderModel = new OrderModel();

            if (!ShowEditor(newOrderModel))
                return;

            IPreliminaryOrder createdOrder;
            try
            {
                createdOrder = PluginContext.Operations.CreatePreliminaryOrder(newOrderModel.Number, newOrderModel.OriginName, newOrderModel.CreateProductDtoItems());
            }
            catch (ConstraintViolationException e)
            {
                MessageBox.Show(
                    "Failed to edit order:" + Environment.NewLine + e.Message,
                    "Edit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            newOrderModel.Update(createdOrder);
            orderModels.Add(newOrderModel);
        }

        private void ButtonEditClick(object sender, RoutedEventArgs args)
        {
            var selectedItem = lstOrders.SelectedItem;
            if (selectedItem == null)
                return;

            var orderModel = (OrderModel)selectedItem;

            var modelCopy = orderModel.CreateCopy();

            if (!ShowEditor(modelCopy))
                return;

            try
            {
                // Выполнение операции может занять длительное время
                // из-за необходимости обращения к удалённой машине,
                // поэтому её лучше выполнять в фоновом потоке.
                PluginContext.Operations.ChangePreliminaryOrder(modelCopy.Source, modelCopy.Number, modelCopy.OriginName, modelCopy.CreateProductDtoItems());
            }
            catch (EntityNotFoundException e)
            {
                orderModels.Remove(orderModel);

                MessageBox.Show(
                    "Order not found:" + Environment.NewLine + e.Message,
                    "Edit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            catch (PreliminaryOrderEditTemporaryUnavailableException e)
            {
                MessageBox.Show(
                    "Failed to edit order due to temporary error. Try to edit order later:" + Environment.NewLine + e.Message,
                    "Edit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            catch (ConstraintViolationException e)
            {
                MessageBox.Show(
                    "Failed to edit order:" + Environment.NewLine + e.Message,
                    "Edit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            var index = orderModels.IndexOf(orderModel);
            if (index != -1)
                orderModels[index] = modelCopy;
        }

        private void ButtonDeleteClick(object sender, RoutedEventArgs args)
        {
            var selectedItem = lstOrders.SelectedItem;
            if (selectedItem == null)
                return;

            var orderModel = (OrderModel)selectedItem;
            var order = orderModel.Source;

            PluginContext.Operations.DeletePreliminaryOrder(order);
            orderModels.Remove(orderModel);
        }
        #endregion
    }
}
