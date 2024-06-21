using Resto.Front.Api.SamplePlugin.WpfHelpers;
using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal sealed class ModifiersViewer : IDisposable
    {
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private readonly object syncObject = new object();
        private bool disposed;

        public ModifiersViewer()
        {
            var windowThread = new Thread(Main);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            PluginContext.Log.Info("ModifiersViewer started");
        }

        private void Main()
        {
            ModifiersWindow modifiersWindow;
            lock (syncObject)
            {
                if (disposed)
                    return;

                DispatcherHelper.Initialize();
                modifiersWindow = new ModifiersWindow();

                resources.Add(Disposable.Create(() =>
                {
                    modifiersWindow.Dispatcher.InvokeShutdown();
                    modifiersWindow.Dispatcher.Thread.Join();
                    DispatcherHelper.Reset();
                }));
            }

            var logger = PluginContext.Log;
            logger.Info("Show ModifiersViewer dialog...");
            modifiersWindow.ShowDialog();
            logger.Info("Closed ModifiersViewer dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;

            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("ModifiersViewer stopped");
                disposed = true;
            }
        }
    }
}
