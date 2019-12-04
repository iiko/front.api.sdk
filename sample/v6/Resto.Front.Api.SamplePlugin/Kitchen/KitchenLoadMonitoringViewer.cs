using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using Resto.Front.Api.Data.Kitchen;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    internal sealed class KitchenLoadMonitoringViewer : IDisposable
    {
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private readonly object syncObject = new object();
        private bool disposed;

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
                    .Select(_ => Unit.Default)
                    .Merge(Observable.Return(Unit.Default))
                    .Select(_ => CalculateStatistics())
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

        private static KitchenLoadMonitoringModel CalculateStatistics()
        {
            var now = DateTime.Now;
            var kitchenOrders = PluginContext.Operations.GetKitchenOrders();
            var orderItemProducts = kitchenOrders
                .SelectMany(order => order.Items)
                .Where(product => !product.Deleted)
                .ToList();
            var processingProducts = orderItemProducts
                .Where(product => IsProcessingStatus(product.ProcessingStatus))
                .ToList();
            var kitchensLoad = processingProducts
                .GroupBy(product => product.Kitchen)
                .ToDictionary(group => group.Key, group => group.Count());

            var cookingProducts = processingProducts.Select(item => new CookingProductModel(item.GetCookingItemName(), item.GetCookingItemAmount())).ToList();

            return new KitchenLoadMonitoringModel
            {
                OrdersCount = kitchenOrders.Count,
                ProductsCount = orderItemProducts.Count,
                IdleProductsCount = orderItemProducts.Count(product => product.ProcessingStatus == KitchenOrderItemProcessingStatus.Idle),
                ProcessingProductsCount = processingProducts.Count,
                ProcessingOverdueProductsCount = processingProducts
                                                    .Where(product => product.CookingTime.HasValue && product.EstimatedCookingBeginTime.HasValue)
                                                    .Count(product => product.EstimatedCookingBeginTime + product.CookingTime < (product.ProcessingCompleteTime ?? now)),
                ProcessedProductsCount = orderItemProducts.Count(product => product.ProcessingStatus == KitchenOrderItemProcessingStatus.Processed),
                ServedProductsCount = orderItemProducts.Count(product => product.ProcessingStatus == KitchenOrderItemProcessingStatus.Served),
                KitchensLoad = kitchensLoad,
                CookingProducts = cookingProducts
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