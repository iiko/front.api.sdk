using System;
using System.Runtime.Remoting;
using System.Windows;
using Resto.Front.Api.Data.Organization;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public class ConnectionToMainTerminalChangedHandler : IDisposable
    {
        private readonly IDisposable subscription;
        private readonly ITerminal mainTerminal;
        private readonly ITerminal hostTerminal;
        private bool lastStateOfConnection = true;
        public ConnectionToMainTerminalChangedHandler()
        {
            mainTerminal = PluginContext.Operations.GetHostTerminalsGroup().MainTerminal;
            hostTerminal = PluginContext.Operations.GetHostTerminal();
            subscription = PluginContext.Notifications.ConnectionToMainTerminalChanged.Subscribe(OnConnectionToMainTerminalChanged);
        }

        private void OnConnectionToMainTerminalChanged(bool isConnected)
        {
            if (lastStateOfConnection == isConnected)
                return;

            switch (isConnected)
            {
                case true:
                    MessageBox.Show($"{hostTerminal.Name} has reconnected with main terminal {mainTerminal.Name}");
                    break;
                case false:
                    MessageBox.Show($"{hostTerminal.Name} has lost connection with main terminal {mainTerminal.Name}");
                    break;
            }

            lastStateOfConnection = isConnected;
        }

        public void Dispose()
        {
            try
            {
                subscription.Dispose();
            }
            catch (RemotingException)
            {
                // nothing to do with the lost connection
            }
        }
    }
}