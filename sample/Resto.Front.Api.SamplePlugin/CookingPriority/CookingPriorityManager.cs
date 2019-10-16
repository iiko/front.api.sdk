using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.CookingPriority
{
    internal sealed class CookingPriorityManager : IDisposable
    {
        private readonly object syncObject = new object();
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private bool disposed;

        public CookingPriorityManager()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            PluginContext.Log.Info("CookingPriorityManager started");
        }

        private void EntryPoint()
        {
            Window window;
            lock (syncObject)
            {
                if (disposed)
                    return;

                var cookingPriorityView = new CookingPriorityView();

                window = new Window
                             {
                                 SizeToContent = SizeToContent.WidthAndHeight,
                                 ResizeMode = ResizeMode.CanResize,
                                 Content = cookingPriorityView,
                                 Title = GetType().Name,
                                 Topmost = true
                             };

                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
                // NOTE: performance warning
                // Do not reload all orders every time in a real production code, only replace single changed order.
                resources.Add(PluginContext.Notifications.OrderChanged
                    .Subscribe(_ => window.Dispatcher.BeginInvoke((Action)(cookingPriorityView.ReloadOrders))));
            }

            PluginContext.Log.Info("Show CookingPriorityView dialog...");
            window.ShowDialog();
            PluginContext.Log.Info("Close CookingPriorityView dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("CookingPriorityView stopped");
                disposed = true;
            }
        }
    }
}
