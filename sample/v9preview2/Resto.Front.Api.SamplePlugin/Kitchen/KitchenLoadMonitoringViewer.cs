using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.Kitchen;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    internal sealed class KitchenLoadMonitoringViewer : IDisposable
    {
        private readonly CompositeDisposable resources = new();
        private readonly object syncObject = new();
        private bool disposed;
        private IReadOnlyCollection<IKitchenOrder> kitchenOrders = PluginContext.Operations.GetKitchenOrders();

        public KitchenLoadMonitoringViewer()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void EntryPoint()
        {
            Window window;

            lock (syncObject)
            {
                if (disposed)
                    return;

                var loadMonitoringView = new KitchenLoadMonitoringView();
                window = new Window
                {
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.CanResize,
                    Content = loadMonitoringView,
                    Title = GetType().Name,
                    Topmost = true
                };
                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
                resources.Add(PluginContext.Notifications.KitchenOrderChanged
                    .ObserveOn(DispatcherScheduler.Current)
                    .Select(args => args.EventType != EntityEventType.Removed ? args.Entity : null)
                    .Merge(Observable.Return((IKitchenOrder)null))
                    .Select(CalculateStatistics)
                    .Subscribe(model => loadMonitoringView.DataContext = model));
            }

            window.ShowDialog();
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                disposed = true;
            }
        }

        private KitchenLoadMonitoringModel CalculateStatistics([CanBeNull] IKitchenOrder changedOrder)
        {
            if (changedOrder != null)
            {
                var cachedOrder = kitchenOrders.SingleOrDefault(o => o.Id == changedOrder.Id);
                if (cachedOrder != null
                    && cachedOrder.Items.Any(o => o.ProcessingStatus != KitchenOrderItemProcessingStatus.Processed)
                    && changedOrder.Items.All(o => o.ProcessingStatus == KitchenOrderItemProcessingStatus.Processed))
                {
                    var doc = PrintDocumentGenerator.GenerateKitchenDocument(changedOrder);
                    var printer = PluginContext.Operations.TryGetDishPrinter(changedOrder.Items[0].Kitchen);
                    if (printer != null)
                        PluginContext.Operations.Print(printer, doc);
                }
            }

            var now = DateTime.Now;
            kitchenOrders = PluginContext.Operations.GetKitchenOrders();
            var orderItemProducts = kitchenOrders
                .SelectMany(order => order.Items.Where(item => !item.Deleted).Select(item => new CookingProductModel(order, item)))
                .ToList();
            var processingProducts = orderItemProducts
                .Select(product => product.CookingItem)
                .Where(product => IsProcessingStatus(product.ProcessingStatus))
                .ToList();
            var kitchensLoad = processingProducts
                .GroupBy(product => product.Kitchen)
                .ToDictionary(group => group.Key, group => group.Count());

            return new KitchenLoadMonitoringModel
            {
                OrdersCount = kitchenOrders.Count,
                ProductsCount = orderItemProducts.Count,
                IdleProductsCount = orderItemProducts.Count(product => product.CookingItem.ProcessingStatus == KitchenOrderItemProcessingStatus.Idle),
                ProcessingProductsCount = processingProducts.Count,
                ProcessingOverdueProductsCount = processingProducts
                    .Where(product => product.CookingTime.HasValue && product.EstimatedCookingBeginTime.HasValue)
                    .Count(product => product.EstimatedCookingBeginTime + product.CookingTime < (product.ProcessingCompleteTime ?? now)),
                ProcessedProductsCount = orderItemProducts.Count(product => product.CookingItem.ProcessingStatus == KitchenOrderItemProcessingStatus.Processed),
                ServedProductsCount = orderItemProducts.Count(product => product.CookingItem.ProcessingStatus == KitchenOrderItemProcessingStatus.Served),
                KitchensLoad = kitchensLoad,
                CookingProducts = orderItemProducts
            };
        }

        private static bool IsProcessingStatus(KitchenOrderItemProcessingStatus status)
        {
            switch (status)
            {
                case KitchenOrderItemProcessingStatus.Processing1:
                case KitchenOrderItemProcessingStatus.Processing2:
                case KitchenOrderItemProcessingStatus.Processing3:
                case KitchenOrderItemProcessingStatus.Processing4:
                    return true;
                default:
                    return false;
            }
        }
    }
}