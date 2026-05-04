using System;
using System.Threading;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.DiagnosticMessagesTester
{
    internal sealed class MessagesTester : IDisposable
    {
        private bool disposed;
        private Window window;

        public MessagesTester()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void EntryPoint()
        {
            window = new Window
            {
                SizeToContent = SizeToContent.WidthAndHeight,
                Content = new MessageSender(),
                Topmost = true
            };

            window.ShowDialog();
        }

        public void Dispose()
        {
            if (disposed)
                return;
            
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();

            disposed = true;
        }
    }
}
