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
            if (subscriptions != null)
                subscriptions.Dispose();
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
                PluginContext.Log.WarnFormat("Payment system '{0}': '{1}' wasn't registered. Reason: {2}", paymentSystem.PaymentSystemKey, paymentSystem.PaymentSystemName, ex.Message);
                return;
            }

            PluginContext.Log.InfoFormat("Payment system '{0}': '{1}' was successfully registered on server.", paymentSystem.PaymentSystemKey, paymentSystem.PaymentSystemName);
            subscriptions.Add(new PaymentEditorTester());
        }
    }
}
