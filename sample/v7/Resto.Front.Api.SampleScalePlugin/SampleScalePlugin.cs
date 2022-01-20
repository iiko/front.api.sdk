using System;
using System.Collections.Generic;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Exceptions;

namespace Resto.Front.Api.SampleScalePlugin
{
    /// <summary>
    /// Test plug-in to demonstrate opportunities of device api.
    /// Copy build result Resto.Front.Api.SampleScalePlugin.dll to \Plugins\Resto.Front.Api.SampleScalePlugin\ folder near iikoFront.exe
    /// </summary>
    [UsedImplicitly]
    [PluginLicenseModuleId(0021023201)]
    public sealed class SampleScalePlugin : IFrontPlugin
    {
        private readonly List<IDisposable> subscriptions;

        public void Dispose()
        {
            if (subscriptions != null)
            {
                foreach (IDisposable subscription in subscriptions)
                {
                    if (subscription != null)
                        subscription.Dispose();
                }
            }
        }

        public SampleScalePlugin()
        {
            subscriptions = new List<IDisposable>();

            var scaleFactory = new SampleScaleFactory();

            try
            {
                subscriptions.Add(PluginContext.Operations.RegisterScaleFactory(scaleFactory));
            }
            catch (LicenseRestrictionException ex)
            {
                PluginContext.Log.Warn(ex.Message);
                PluginContext.Shutdown();
                return;
            }
            catch (DeviceRegistrationException ex)
            {
                PluginContext.Log.InfoFormat("SampleScale factory: '{0}' wasn't registered. Reason: {1}", scaleFactory.CodeName, ex.Message);
                PluginContext.Shutdown();
                return;
            }

            PluginContext.Log.InfoFormat("SampleScale factory: '{0}' was successfully registered", scaleFactory.CodeName);
        }
    }
}
