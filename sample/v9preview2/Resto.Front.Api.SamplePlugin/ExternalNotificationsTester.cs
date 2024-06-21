using System;
using System.Linq;

namespace Resto.Front.Api.SamplePlugin;

internal static class ExternalNotificationsTester
{
    public static IDisposable Test()
    {
        const string subscriptionName = "ExternalNotification";

        var notificationListener = new NotificationListener(subscriptionName);
        var notificationInvoker = new NotificationInvoker();
        notificationInvoker.InvokeExternalNotification();

        return notificationListener;
    }

    private sealed class NotificationInvoker
    {
        public void InvokeExternalNotification()
        {
            //Can be used either GetAllExternalNotificationsNames or a known subscription name
            var subscriptionName = PluginContext.Operations.GetAllExternalNotificationsNames().Last();
            PluginContext.Operations.InvokeExternalNotification(subscriptionName, Guid.NewGuid().ToString());
        }
    }

    private sealed class NotificationListener : IDisposable
    {
        private readonly IDisposable subscription;

        public NotificationListener(string subscriptionName)
        {
            subscription =
                PluginContext.Operations
                    .GetExternalNotificationSubscription(subscriptionName)
                    .Subscribe(OnExternalNotificationSubscription);
        }

        private static void OnExternalNotificationSubscription(string message)
        {
            PluginContext.Log.Info($"Listener received notification: \"{message}\"");
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}