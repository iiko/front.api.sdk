using System.Reactive.Disposables;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Exceptions;

namespace Resto.Front.Api.SamplePaymentPlugin
{
    /// <summary>
    /// Test plug-in to demonstrate opportunities of payment system api.
    /// Copy build result Resto.Front.Api.SamplePaymentPlugin.dll to \Plugins\Resto.Front.Api.SamplePaymentPlugin\ folder near iikoFront.exe
    /// </summary>
    [UsedImplicitly]
    [PluginLicenseModuleId(0021005918)]
    public sealed class SamplePaymentPlugin : IFrontPlugin
    {
        private readonly CompositeDisposable subscriptions;

        public void Dispose()
        {
            subscriptions?.Dispose();
        }

        public SamplePaymentPlugin()
        {
            subscriptions = new CompositeDisposable();
            var paymentSystem = new ExternalPaymentProcessorSample();

            subscriptions.Add(paymentSystem);
            try
            {
                subscriptions.Add(PluginContext.Operations.RegisterPaymentSystem(paymentSystem));
            }
            catch (LicenseRestrictionException ex)
            {
                PluginContext.Log.Warn(ex.Message);
                return;
            }
            catch (PaymentSystemRegistrationException ex)
            {
                PluginContext.Log.Warn($"Payment system '{paymentSystem.PaymentSystemKey}': '{paymentSystem.PaymentSystemName}' wasn't registered. Reason: {ex.Message}");
                return;
            }

            PluginContext.Log.Info($"Payment system '{paymentSystem.PaymentSystemKey}': '{paymentSystem.PaymentSystemName}' was successfully registered on server.");
            subscriptions.Add(new PaymentEditorTester());
        }
    }
}
