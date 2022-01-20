using System;
using System.Runtime.Remoting;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Organization.Sections;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public class TableChangedHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public TableChangedHandler()
        {
            subscription = PluginContext.Notifications.TableChanged.Subscribe(s => OnTableChanged(s.Entity));
        }

        private void OnTableChanged([NotNull] ITable table)
        {
            PluginContext.Log.Info("On table changed subscription.");
            var restaurantSection = table.RestaurantSection;
            var message = restaurantSection == null
                ? "There is no restaurant section"
                : $"Current restaurant section: {restaurantSection.Name}";
            MessageBox.Show(message, "", MessageBoxButton.OK);
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
