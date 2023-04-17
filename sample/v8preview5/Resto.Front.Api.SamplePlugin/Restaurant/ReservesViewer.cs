using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal sealed class ReservesViewer : IDisposable
    {
        private readonly object syncObject = new object();
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private bool disposed;

        public ReservesViewer()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            PluginContext.Log.Info("ReservesViewer started");
        }

        private void EntryPoint()
        {
            Window window;
            lock (syncObject)
            {
                if (disposed)
                    return;

                var reservesView = new ReservesView();

                window = new Window
                             {
                                 SizeToContent = SizeToContent.WidthAndHeight,
                                 ResizeMode = ResizeMode.CanResize,
                                 Content = reservesView,
                                 Title = GetType().Name,
                                 Topmost = true
                             };

                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
                resources.Add(PluginContext.Notifications.ReserveChanged
                    .Subscribe(_ => window.Dispatcher.BeginInvoke((Action)(reservesView.ReloadReserves))));
            }

            PluginContext.Log.Info("Show ReservesView dialog...");
            window.ShowDialog();
            PluginContext.Log.Info("Close ReservesView dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("ReservesViewer stopped");
                disposed = true;
            }
        }
    }
}
