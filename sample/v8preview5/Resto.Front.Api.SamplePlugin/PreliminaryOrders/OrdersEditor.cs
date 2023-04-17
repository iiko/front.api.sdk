using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;


namespace Resto.Front.Api.SamplePlugin.PreliminaryOrders
{
    internal sealed class OrdersEditor : IDisposable
    {
        #region Fields
        private readonly object syncObject = new object();
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private bool disposed;
        #endregion

        public OrdersEditor()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();

            PluginContext.Operations.EnablePreliminaryOrdersScreen();

            PluginContext.Log.Info("OrdersEditor started");
        }

        private void EntryPoint()
        {
            Window window;
            lock (syncObject)
            {
                if (disposed)
                    return;

                var view = new OrdersView();
                window = new Window
                {
                    MinHeight = 480,
                    MinWidth = 640,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Content = view,
                    Title = GetType().Name
                };

                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
                resources.Add(view);
            }
            PluginContext.Log.Info("Show OrdersEditor dialog...");
            window.ShowDialog();
            PluginContext.Log.Info("Closed OrdersEditor dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("OrdersEditor stopped");
                disposed = true;
            }
        }
    }
}