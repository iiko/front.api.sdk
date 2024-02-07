using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Device.Results;
using Resto.Front.Api.Data.Device.Tasks;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Devices;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class ChequeTaskProcessor : IChequeTaskProcessor
    {
        public bool IsPrimary => false;

        public void BeforePayIn(ICashRegisterInfo device, decimal sumToPayIn, bool isCloseSession, IUser cashier, IViewManager viewManager) { }

        public void BeforePayOut(ICashRegisterInfo device, decimal availableSum, ref decimal sumToPayOut, bool isCloseSession, IUser cashier, IViewManager viewManager) { }

        public BeforeDoCheckActionResult BeforeDoCheckAction(ChequeTask chequeTask, ICashRegisterInfo device, CashRegisterChequeExtensions chequeExtensions, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Before do cheque on cash register: {0} ({1})", device.FriendlyName, device.Id);
            const bool needChangeInfo = true;
            if (needChangeInfo)
            {
                var sampleBeginElement = new XElement(Tags.Left,
                    new XElement(Tags.Left, "Organization"),
                    new XElement(Tags.Left, "Address"),
                    new XElement(Tags.Left, $"Table number: {chequeTask.TableNumberLocalized}"));
                var textBeforeCheque = sampleBeginElement;
                var sampleAfterElement = new XElement(Tags.Left,
                    new XElement(Tags.Left, "Have a nice day"));
                var textAfterCheque = sampleAfterElement;

                // This information will be passed to the front app
                return new BeforeDoCheckActionResult
                {
                    BeforeCheque = new List<Data.Print.Document>(new[] { new Data.Print.Document { Markup = textBeforeCheque } }), // markup to add to the header (at the very beginning of the cheque)
                    AfterCheque = new List<Data.Print.Document>(new[] { new Data.Print.Document { Markup = textAfterCheque } }), // markup to add at the end of the cheque
                    CashierName = "CashierName",
                    CustomerTin = "123456789012"
                };
            }

            // Nothing to change
            return null;
        }

        public void BeforeZReport(ICashRegisterInfo device, decimal cashRest, IUser authUser, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Before print z-report on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void BeforeXReport(ICashRegisterInfo device, IUser authUser, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Before print x-report on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void BeforeOpenSession(ICashRegisterInfo device, IUser authUser, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Before open session on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void AfterDoCheckAction(ChequeTask chequeTask, PostResult result, ICashRegisterInfo device, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("After do cheque on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void AfterZReport(ICashRegisterInfo device, PostResult result, IUser authUser, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("After print z-report on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void AfterXReport(ICashRegisterInfo device, PostResult result, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("After print x-report on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void AfterPayIn(ICashRegisterInfo device, decimal sum, PostResult result, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("After pay in on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void AfterPayOut(ICashRegisterInfo device, decimal sum, PostResult result, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("After pay in out cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public void AfterOpenSession(ICashRegisterInfo device, PostResult result, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("After open session on cash register: {0} ({1})", device.FriendlyName, device.Id);
        }

        public static IDisposable Register()
        {
            IDisposable subscription = null;
            try
            {
                subscription = PluginContext.Operations.RegisterChequeTaskProcessor(new ChequeTaskProcessor());
                PluginContext.Log.Warn("ChequeTaskProcessor was registered.");
            }
            catch (LicenseRestrictionException ex)
            {
                PluginContext.Log.Warn(ex.Message);
            }

            return subscription;
        }
    }
}
