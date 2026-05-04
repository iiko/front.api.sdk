using System;
using System.Globalization;
using System.Threading;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin
{
    using static PluginContext;

    internal sealed class LocalizationTester : IDisposable
    {
        [NotNull]
        private readonly IDisposable subscription;

        public LocalizationTester()
        {
            subscription = Notifications.CurrentCultureChanged.Subscribe(x =>
            {
                CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture = x.culture;
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = x.uiCulture;
            });
        }

        public void Dispose() => subscription.Dispose();
    }
}
