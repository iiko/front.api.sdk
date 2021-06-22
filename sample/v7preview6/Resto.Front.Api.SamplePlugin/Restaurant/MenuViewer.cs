using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal sealed class MenuViewer : IDisposable
    {
        private readonly object syncObject = new object();
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private bool disposed;

        public MenuViewer()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            PluginContext.Log.Info("MenuViewer started");
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
                    Content = new MenuView(),
                    Title = GetType().Name,
                    Topmost = true
                };

                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
            }
            PluginContext.Log.Info("Show MenuViewer dialog...");
            window.ShowDialog();
            PluginContext.Log.Info("Closed MenuViewer dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("MenuViewer stopped");
                disposed = true;
            }
        }
    }
}
