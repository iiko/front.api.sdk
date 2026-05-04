using System;
using Resto.Front.Api.Data.Device;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public sealed class CardScannedHandler : IDisposable
    {
        private readonly IDisposable cardScanningSubscription;
        private readonly IDisposable cardScanningFailedSubscription;
        private readonly IDisposable cardScannedSubscription;

        public CardScannedHandler()
        {
            cardScanningSubscription = PluginContext.Notifications.CardScanning.Subscribe(_ => OnCardScanning());
            cardScanningFailedSubscription = PluginContext.Notifications.CardScanningFailed.Subscribe(_ => OnCardScanningFailed());
            cardScannedSubscription = PluginContext.Notifications.CardScanned.Subscribe(OnCardScanned);
        }

        private static void OnCardScanning()
        {
            PluginContext.Log.Info("Card scanning started.");
        }

        private static void OnCardScanningFailed()
        {
            PluginContext.Log.Warn("Card scanning failed — card was not read by hardware.");
        }

        private static bool OnCardScanned(CardScannedEventArgs args)
        {
            PluginContext.Log.Info($"Card scanned: DeviceType={args.DeviceType}, Data={args.Data}");

            // return true to stop further processing
            return false;
        }

        public void Dispose()
        {
            try { cardScanningSubscription.Dispose(); } catch (Exception) { }
            try { cardScanningFailedSubscription.Dispose(); } catch (Exception) { }
            try { cardScannedSubscription.Dispose(); } catch (Exception) { }
        }
    }
}
