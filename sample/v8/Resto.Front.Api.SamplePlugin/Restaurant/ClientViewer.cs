using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal sealed class ClientViewer : IDisposable
    {
        private readonly object syncObject = new object();
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private bool disposed;

        public ClientViewer()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            PluginContext.Log.Info("ClientViewer started");
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
                                 ResizeMode = ResizeMode.NoResize,
                                 Content = new ClientView(),
                                 Title = GetType().Name,
                                 Topmost = true

                             };

                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
            }

            PluginContext.Log.Info("Show ClientView dialog...");
            window.ShowDialog();
            PluginContext.Log.Info("Close ClientView dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("ClientViewer stopped");
                disposed = true;
            }
        }
    }
}
