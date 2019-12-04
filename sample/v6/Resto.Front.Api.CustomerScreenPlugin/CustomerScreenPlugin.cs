using System;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using Resto.Front.Api.CustomerScreen.Settings;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.CustomerScreen.Helpers;
using Resto.Front.Api.CustomerScreen.View;
using Resto.Front.Api.CustomerScreen.ViewModel;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Screens;

namespace Resto.Front.Api.CustomerScreen
{
    [UsedImplicitly, PluginLicenseModuleId(ModuleId)]
    public sealed class CustomerScreenPlugin : IFrontPlugin
    {
        private const int ModuleId = 10000;
        private readonly CompositeDisposable unsubscribe = new CompositeDisposable();
        private static Order vmOrder;
        private static CustomerScreenWindow customerScreen;
        public static ICurrencySettings CurrencySettings;

        public CustomerScreenPlugin()
        {
            var screenHelper = new ScreenHelper();
            if (!screenHelper.IsSecondMonitorExists)
            {
                PluginContext.Shutdown("Can't show customer screen. There is no second monitor.");
                return;
            }

            try
            {
                CustomerScreenConfig.Init(PluginContext.Integration.GetConfigsDirectoryPath());
                InitializeUiDispatcher(PluginContext.Operations.GetHostTerminalCultureInfo());
                CurrencySettings = PluginContext.Operations.GetHostRestaurant().Currency;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    vmOrder = new Order();

                    unsubscribe.Add(PluginContext.Notifications.ScreenChanged
                        .ObserveOn(DispatcherScheduler.Current)
                        .Subscribe(OnScreenChanged));
                    unsubscribe.Add(PluginContext.Notifications
                        .ChangeSumChanged.ObserveOn(DispatcherScheduler.Current)
                        .Subscribe(OnChangeSumChanged));
                    unsubscribe.Add(PluginContext.Notifications.OrderChanged
                        .ObserveOn(DispatcherScheduler.Current)
                        .Where(e => Equals(vmOrder.OrderSource, e.Entity))
                        .Select(e => e.Entity)
                        .Subscribe(vmOrder.Update));
                    unsubscribe.Add(PluginContext.Notifications.RestaurantChanged
                        .ObserveOn(DispatcherScheduler.Current)
                        .Subscribe(r => OnRestaurantChanged(r.Currency)));

                    ShowCustomerScreen(screenHelper);
                });
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private static void ShowCustomerScreen(ScreenHelper screenHelper)
        {
            customerScreen = new CustomerScreenWindow
            {
                Top = screenHelper.SecondMonitorTop,
                Left = screenHelper.SecondMonitorLeft,
                DataContext = vmOrder
            };
            PluginContext.Log.Info("Customer screen was created.");
            customerScreen.Show();
            PluginContext.Log.Info("Customer screen was shown.");

            customerScreen.WindowState = WindowState.Maximized;
            customerScreen.mediaControl.StartPlayer();
        }

        private static void OnScreenChanged(IScreen screen)
        {
            PluginContext.Log.Info(screen.ToString());
            var swo = screen as IScreenWithOrder;
            if (swo != null)
            {
                PluginContext.Log.Info(swo.Order.Number.ToString());
                vmOrder.Update(swo.Order);
            }
            else
                vmOrder.Reset();
        }

        private static void OnChangeSumChanged(decimal sum)
        {
            customerScreen.ChangeSumChanged(sum);
        }

        private static void OnRestaurantChanged(ICurrencySettings currencySettings)
        {
            CurrencySettings = currencySettings;
        }

        public void Dispose()
        {
            ShutdownUiDispatcher();
            unsubscribe.Dispose();
        }

        private static void InitializeUiDispatcher(CultureInfo uiCulture)
        {
            using (var uiThreadStartedSignal = new ManualResetEvent(false))
            {
                var thread = new Thread(() =>
                {
                    var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                    StartupEventHandler startupHandler = null;
                    startupHandler = (s, e) =>
                    {
                        // ReSharper disable once AccessToDisposedClosure (calling thread wouldn't exit from 'using' until we call 'Set')
                        uiThreadStartedSignal.Set();
                        app.Startup -= startupHandler;
                    };
                    app.Startup += startupHandler;
                    app.Run();
                });
                thread.CurrentUICulture = uiCulture;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                uiThreadStartedSignal.WaitOne();
            }
        }

        private static void ShutdownUiDispatcher()
        {
            if (Application.Current == null)
                return;

            Application.Current.Dispatcher.InvokeShutdown();
            Application.Current.Dispatcher.Thread.Join();
        }
    }
}