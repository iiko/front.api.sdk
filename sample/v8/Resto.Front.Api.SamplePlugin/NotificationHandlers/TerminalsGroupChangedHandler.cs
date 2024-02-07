using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Organization;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public class TerminalsGroupChangedHandler : IDisposable
    {
        private readonly IDisposable subscription;
        private Dictionary<Guid, ITerminalsGroup> terminalsGroups;

        public TerminalsGroupChangedHandler()
        {
            subscription = PluginContext.Notifications.TerminalsGroupChanged.Subscribe(OnTerminalsGroupChanged);
            terminalsGroups = PluginContext.Operations.GetTerminalsGroups().ToDictionary(i => i.Id, g => g);
        }

        private void OnTerminalsGroupChanged([NotNull] ITerminalsGroup terminalsGroup)
        {
            PluginContext.Log.Info("On terminals group changed subscription.");
            if (terminalsGroups.ContainsKey(terminalsGroup.Id))
            {
                var newMainTerminal = terminalsGroup.MainTerminal;
                var oldMainTerminal = terminalsGroups[terminalsGroup.Id].MainTerminal;
                switch (oldMainTerminal, newMainTerminal)
                {
                    case (null, not null):
                        MessageBox.Show($"New main terminal: {newMainTerminal.Name} - {newMainTerminal.Id}", "", MessageBoxButton.OK);
                        break;
                    case (not null, null):
                        MessageBox.Show($"Now terminals group {terminalsGroup.Id} has no main terminal", "", MessageBoxButton.OK);
                        break;
                    case (not null, not null):
                        if (oldMainTerminal.Id != newMainTerminal.Id)
                            MessageBox.Show($"New main terminal: {newMainTerminal.Name} - {newMainTerminal.Id}", "", MessageBoxButton.OK);
                        break;
                }
            }

            terminalsGroups = PluginContext.Operations.GetTerminalsGroups().ToDictionary(i => i.Id, g => g);
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