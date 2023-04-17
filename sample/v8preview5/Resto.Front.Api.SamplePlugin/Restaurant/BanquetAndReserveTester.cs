using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal sealed class BanquetAndReserveTester : IDisposable
    {
        private readonly object syncObject = new object();
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private bool disposed;

        public BanquetAndReserveTester()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            PluginContext.Log.Info("BanquetAndReserveTester started");
        }

        private void EntryPoint()
        {
            Window window;
            lock (syncObject)
            {
                if (disposed)
                    return;

                window = new Window
                {
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Content = new BanquetAndReserveView(),
                    Title = GetType().Name,
                    Topmost = true
                };

                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
            }
            PluginContext.Log.Info("Show BanquetAndReserveView dialog...");
            window.ShowDialog();
            PluginContext.Log.Info("Closed BanquetAndReserveView dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("BanquetAndReserveTester stopped");
                disposed = true;
            }
        }
    }
}
