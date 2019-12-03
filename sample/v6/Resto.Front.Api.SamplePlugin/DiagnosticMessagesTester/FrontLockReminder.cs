using System;
using System.Reactive.Linq;
using Resto.Front.Api.Data.Screens;


namespace Resto.Front.Api.SamplePlugin.DiagnosticMessagesTester
{
    internal sealed class FrontLockReminder : IDisposable
    {
        private readonly IDisposable subscription;

        // Напоминание заблокировать экран при каждом переходе на экран заказа
        public FrontLockReminder()
        {
            const string message = "Не забудь заблокировать экран!";

            subscription = PluginContext.Notifications.ScreenChanged
                .Where(screen => screen is IOrderEditScreen)
                .Subscribe(_ => PluginContext.Operations.AddNotificationMessage(message, "SamplePlugin", TimeSpan.FromSeconds(15)));
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}
