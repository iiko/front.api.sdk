using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.DataTransferObjects;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.UI;
using System;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public sealed class GetEInvoiceNumberHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public GetEInvoiceNumberHandler()
        {
            subscription = PluginContext.Notifications.GetEInvoiceNumber.Subscribe(x => OnGetEInvoiceNumber(x.orderId, x.vm));
        }

        private static GetEInvoiceNumberResult OnGetEInvoiceNumber(Guid orderId, [CanBeNull] IViewManager vm)
        {
            // Имитируем получение номера инвойса и отправку на фронт.
            PluginContext.Log.Info("On get EInvoice number via plugin subscription.");

            // В V9 тип EInvoiceNumber изменён с int на string.
            // Для обратной совместимости в V9Preview6 тип EInvoiceNumber оставлен int.
            // В V9Preview6 добавлено свойство string EInvoiceNumberString.
            // В V9Preview6, если фронт получит EInvoiceNumberString не null, то как номер инвойса будет использован EInvoiceNumberString.
            // Если значение EInvoiceNumberString null и Succeeded == true, то как номер инвойса будет использовано значение EInvoiceNumber.
            var result = new GetEInvoiceNumberResult();
            //try
            //{
            //    var dialogResult = vm.ShowKeyboard("Enter EInvoce Number", "");
            //    if (dialogResult is not null)
            //        result.EInvoiceNumber = dialogResult;
            //    else
            //    {
            //        PluginContext.Log.Warn("EInvoice number input was cancelled or returned an unexpected result.");
            //        return result;
            //    }

            //    result.Succeeded = true;
            //}
            //catch (Exception e)
            //{
            //    var message = $"Exception in SamplePlugin plugin's handler for OnGetEInvoiceNumberViaPlugin: {e}";
            //    PluginContext.Log.WarnFormat(message);
            //    result.EInvoiceNumber = null;
            //    result.Succeeded = false;
            //    result.Message = message;
            //}

            //PluginContext.Log.Info($"On get EInvoice number via plugin subscription return EInvoiceNumber: {result.EInvoiceNumber} .");
            return result;
        }

        public void Dispose()
        {
            try
            {
                subscription.Dispose();
            }
            catch (Exception)
            {
                // nothing to do with the lost connection
            }
        }
    }
}
