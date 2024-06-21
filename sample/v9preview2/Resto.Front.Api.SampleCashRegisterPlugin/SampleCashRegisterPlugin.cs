﻿using System;
using System.Collections.Generic;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Exceptions;

namespace Resto.Front.Api.SampleCashRegisterPlugin
{
    [UsedImplicitly]
    [PluginLicenseModuleId(0021023101)]
    public sealed class SampleCashRegisterPlugin : IFrontPlugin
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

        public SampleCashRegisterPlugin()
        {
            subscriptions = new List<IDisposable>();

            var cashRegisterFactory = new SampleCashRegisterFactory();

            try
            {
                subscriptions.Add(PluginContext.Operations.RegisterCashRegisterFactory(cashRegisterFactory));
            }
            catch (LicenseRestrictionException ex)
            {
                PluginContext.Log.Warn(ex.Message);
                PluginContext.Shutdown();
                return;
            }
            catch (PaymentSystemRegistrationException ex)
            {
                PluginContext.Log.Warn($"Sample cash register registration error: factory code={cashRegisterFactory.FactoryCode}, " +
                                       $"description={cashRegisterFactory.Description}, reason: {ex.Message}");
                PluginContext.Shutdown();
                return;
            }

            PluginContext.Log.Info($"Sample cash register factory: code={cashRegisterFactory.FactoryCode}," +
                                         $"name='{cashRegisterFactory.Description}' was successfully registered on server.");
        }
    }
}
