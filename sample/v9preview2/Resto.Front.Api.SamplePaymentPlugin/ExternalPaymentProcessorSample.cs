﻿using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Xml.Linq;
using System.Threading;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePaymentPlugin
{
    internal sealed class ExternalPaymentProcessorSample : IPaymentProcessor, IDisposable
    {
        private const string paymentSystemKey = "sample";
        private const string paymentSystemName = "Sample Api Payment";
        private readonly CompositeDisposable subscriptions;

        public ExternalPaymentProcessorSample()
        {
            subscriptions = new CompositeDisposable
            {
                PluginContext.Notifications.CafeSessionClosing.Subscribe(x => CafeSessionClosing(x.printer)),
                PluginContext.Notifications.CafeSessionOpening.Subscribe(x => CafeSessionOpening()),
                PluginContext.Notifications.NavigatingToPaymentScreen.Subscribe(x => NavigatingToPaymentScreen(x.order, x.os)),
            };
        }

        public string PaymentSystemKey => paymentSystemKey;

        public string PaymentSystemName => paymentSystemName;

        // Implementation of collect data method. Executes when plug-in payment item is going to be added to the order.
        public void CollectData(
            Guid orderId,
            Guid paymentTypeId,
            [NotNull] IUser cashier,
            IReceiptPrinter printer,
            IViewManager viewManager,
            IPaymentDataContext context)
        {
            PluginContext.Log.InfoFormat("Collect data for order ({0})", orderId);

            // Show dialog window with magnet card slide listener and number input keyboard.
            var defaultRoomNumber = 42;
            var input = viewManager.ShowInputDialog("Enter room number or slide card", InputDialogTypes.Card | InputDialogTypes.Number, defaultRoomNumber);
            //Cancel button pressed, cancel operation silently.
            if (input == null)
                throw new PaymentActionCancelledException();

            var data = string.Empty;

            // If number was typed input result is INumberInputResult
            if (input is NumberInputDialogResult roomNum)
                data = roomNum.Number.ToString();

            // If card was slided input result is ICardInputResult
            var card = input as CardInputDialogResult;
            if (card != null)
                data = card.FullCardTrack;

            //Required data is not collected, cancel operation with message box.
            if (string.IsNullOrEmpty(data))
                throw new PaymentActionFailedException("Fail to collect data. This text will be shown in dialog window and storning operation will be aborted.");

            // Changing the text displayed on progress bar
            viewManager.ChangeProgressBarMessage("Long action. Information for user");
            Thread.Sleep(5000);

            PluginContext.Log.InfoFormat("Data  {0}, Order id  {1}", data, orderId);

            var d = new CollectedDataDemoClass { Data = data, IsCard = card != null };
            context.SetRollbackData(d);
        }

        // Implementation of collect data method. Executes after plug-in payment item is added to the order on payment page or on preliminary payments page.
        // To do operations with order that is currently edited by operator, we need special operationService. Do NOT use it after method returns.
        public void OnPaymentAdded(
            [NotNull] IOrder order,
            [NotNull] IPaymentItem paymentItem,
            [NotNull] IUser cashier,
            [NotNull] IOperationService operations,
            IReceiptPrinter printer,
            [NotNull] IViewManager viewManager,
            IPaymentDataContext context)
        {
            switch (order.Status)
            {
                case OrderStatus.New:
                    // let's add a gift product to the first guest
                    var credentials = operations.GetDefaultCredentials();

                    var guest = order.Guests.First(); // lucky guest
                    var product = operations.GetActiveProducts().Last(); // gift product
                    operations.AddOrderProductItem(3m, product, order, guest, null, credentials);
                    var orderWithGift = operations.GetOrderById(order.Id);

                    //we assume our gift product is payed by our payment, so automatically add its price to payment sum.
                    var giftSum = orderWithGift.ResultSum - order.ResultSum;
                    paymentItem = orderWithGift.Payments.Single(p => p.Id == paymentItem.Id);
                    operations.ChangePaymentItemSum(paymentItem.Sum + giftSum, giftSum, null, paymentItem, orderWithGift, credentials);
                    return;
                case OrderStatus.Bill:
                    // we cannot edit order widely after bill, but at least we can set possible sum range and provide initial sum value
                    operations.ChangePaymentItemSum(42m, 0m, 100500m, paymentItem, order, operations.GetDefaultCredentials());
                    return;
                default:
                    // we don't expect payment item to be added in statuses other than new and bill
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Implementation of preliminary payment editing method. Executes when plug-in preliminary payment item is going to be edited on preliminary payments page,
        // immediately after user presses the edit button.
        public bool OnPreliminaryPaymentEditing(
            [NotNull] IOrder order,
            [NotNull] IPaymentItem paymentItem,
            [NotNull] IUser cashier,
            [NotNull] IOperationService operationService,
            IReceiptPrinter printer,
            [NotNull] IViewManager viewManager,
            IPaymentDataContext context)
        {
            //enabled standard numpad editing.
            return true;
        }

        public void ReturnPaymentWithoutOrder(
            decimal sum,
            Guid? orderId,
            Guid paymentTypeId,
            [NotNull] IPointOfSale pointOfSale,
            [NotNull] IUser cashier,
            IReceiptPrinter printer,
            IViewManager viewManager)
        {
            if (viewManager == null)
                throw new ArgumentNullException(nameof(viewManager), "Argument is null");
            // if you need to implement return payment without iiko order, you should register your IExternalPaymentProcessor as 'canProcessPaymentReturnWithoutOrder = true'
            throw new PaymentActionFailedException("Not supported action");
        }

        // Implementation of payment method. Executes when order contains plug-in payment item type and order payment process begins.
        public void Pay(
            decimal sum,
            [NotNull] IOrder order,
            [NotNull] IPaymentItem paymentItem,
            Guid transactionId,
            [NotNull] IPointOfSale pointOfSale,
            [NotNull] IUser cashier,
            [NotNull] IOperationService operations,
            IReceiptPrinter printer,
            [NotNull] IViewManager viewManager,
            IPaymentDataContext context)
        {
            PluginContext.Log.InfoFormat("Pay {0}", sum);

            var data = context.GetRollbackData<CollectedDataDemoClass>();

            // Changing the text displayed on progress bar on pay operation
            viewManager.ChangeProgressBarMessage("Printing slip");

            // Slip to print. Slip consists of XElement children from Resto.CashServer.Agent.Print.Tags.Xml (Resto.Framework.dll)
            var slip = new ReceiptSlip
            {
                Doc =
                    new XElement(Tags.Doc,
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Payment System"),
                            new XAttribute(Data.Cheques.Attributes.Right, PaymentSystemKey),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Transaction ID"),
                            new XAttribute(Data.Cheques.Attributes.Right, transactionId.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Data"),
                            new XAttribute(Data.Cheques.Attributes.Right, data != null ? data.Data : "unknown"),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "was card"),
                            new XAttribute(Data.Cheques.Attributes.Right, data != null && data.IsCard ? "YES" : "NO"),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Order #"),
                            new XAttribute(Data.Cheques.Attributes.Right, order.Number.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Full sum"),
                            new XAttribute(Data.Cheques.Attributes.Right, order.FullSum.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Sum to pay"),
                            new XAttribute(Data.Cheques.Attributes.Right, order.ResultSum.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Sum to process"),
                            new XAttribute(Data.Cheques.Attributes.Right, sum.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)))
            };
            printer.Print(slip);
            context.SetInfoForReports(data?.Data, "Test Card Type");

            var donationType = operations.GetDonationTypesCompatibleWith(order).FirstOrDefault(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.External));
            if (donationType != null)
            {
                var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
                var additionalData = new ExternalPaymentItemAdditionalData { CustomData = Serializer.Serialize(new PaymentAdditionalData { SilentPay = true }) };
                var credentials = operations.GetDefaultCredentials();

                operations.AddDonation(order, donationType, paymentType, additionalData, false, order.ResultSum / 2, credentials);
            }
        }

        // Implementation of emergency cancel payment method. Executes when already processed plug-in payment item is removing from order.
        // May be the same method with ReturnPayment() if there is no difference in payment system guidelines.
        public void EmergencyCancelPayment(
            decimal sum,
            Guid? orderId,
            Guid paymentTypeId,
            Guid transactionId,
            [NotNull] IPointOfSale pointOfSale,
            [NotNull] IUser cashier,
            IReceiptPrinter printer,
            IViewManager viewManager,
            IPaymentDataContext context)
        {
            PluginContext.Log.InfoFormat("Cancel {0}", sum);
            ReturnPayment(sum, orderId, paymentTypeId, transactionId, pointOfSale, cashier, printer, viewManager, context);
        }

        public void PaySilently(
            decimal sum,
            [NotNull] IOrder order,
            [NotNull] IPaymentItem paymentItem,
            Guid transactionId,
            [NotNull] IPointOfSale pointOfSale,
            [NotNull] IUser cashier,
            IReceiptPrinter printer,
            IPaymentDataContext context)
        {
            PluginContext.Log.Info("SilentPay");

            var data = context.GetCustomData();

            // You can't change the text displayed on progress bar while on silent pay operation
            //progressBar.ChangeMessage("Printing slip");

            // Slip to print. Slip consists of XElement children from Resto.CashServer.Agent.Print.Tags.Xml (Resto.Framework.dll)
            var slip = new ReceiptSlip
            {
                Doc =
                    new XElement(Tags.Doc,
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Payment System"),
                            new XAttribute(Data.Cheques.Attributes.Right, PaymentSystemKey),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Transaction ID"),
                            new XAttribute(Data.Cheques.Attributes.Right, transactionId.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Data"),
                            new XAttribute(Data.Cheques.Attributes.Right, data ?? "unknown"),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Order #"),
                            new XAttribute(Data.Cheques.Attributes.Right, order.Number.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Full sum"),
                            new XAttribute(Data.Cheques.Attributes.Right, order.FullSum.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Sum to pay"),
                            new XAttribute(Data.Cheques.Attributes.Right, order.ResultSum.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                        new XElement(Tags.Pair,
                            new XAttribute(Data.Cheques.Attributes.Left, "Sum to process"),
                            new XAttribute(Data.Cheques.Attributes.Right, sum.ToString()),
                            new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)))
            };
            printer.Print(slip);
            context.SetInfoForReports(data, "Custom data");
        }

        public void EmergencyCancelPaymentSilently(
            decimal sum,
            Guid? orderId,
            Guid paymentTypeId,
            Guid transactionId,
            [NotNull] IPointOfSale pointOfSale,
            [NotNull] IUser cashier,
            IReceiptPrinter printer,
            IPaymentDataContext context)
        {
            PluginContext.Log.InfoFormat("Silent cancel {0}", sum);
            //there are no instances of viewManager and progressBar
            ReturnPayment(sum, orderId, paymentTypeId, transactionId, pointOfSale, cashier, printer, null, context);
        }

        // Implementation of return payment method. Executes when order contains processed plug-in payment item and order is storning.
        public void ReturnPayment(
            decimal sum,
            Guid? orderId,
            Guid paymentTypeId,
            Guid transactionId,
            [NotNull] IPointOfSale pointOfSale,
            [NotNull] IUser cashier,
            IReceiptPrinter printer,
            [NotNull] IViewManager viewManager,
            IPaymentDataContext context)
        {
            PluginContext.Log.InfoFormat("Order id  {0}", orderId);
            var data = context.GetRollbackData<CollectedDataDemoClass>();
            var slip = new ReceiptSlip
            {
                Doc = new XElement(Tags.Doc,
                    new XElement(Tags.Pair,
                        new XAttribute(Data.Cheques.Attributes.Left, "Return"),
                        new XAttribute(Data.Cheques.Attributes.Right, PaymentSystemKey),
                        new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                    new XElement(Tags.Pair,
                        new XAttribute(Data.Cheques.Attributes.Left, "Transaction ID"),
                        new XAttribute(Data.Cheques.Attributes.Right, transactionId.ToString()),
                        new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                    new XElement(Tags.Pair,
                        new XAttribute(Data.Cheques.Attributes.Left, "was card"),
                        new XAttribute(Data.Cheques.Attributes.Right, data != null && data.IsCard ? "YES" : "NO"),
                        new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)))
            };
            printer.Print(slip);

            // To abort any not successful payment action PaymentActionFailedException should be used.
            var success = true;
            if (!success)
                throw new PaymentActionFailedException("Fail to storno payment. This text will be shown in dialog window and storning operation will be aborted.");
        }

        // Implementation of return payment silently method. Executes when order contains processed plug-in payment item and order is storning.
        public void ReturnPaymentSilently(
            decimal sum,
            Guid? orderId,
            Guid paymentTypeId,
            Guid transactionId,
            [NotNull] IPointOfSale pointOfSale,
            [NotNull] IUser cashier,
            IReceiptPrinter printer,
            IPaymentDataContext context)
        {
            PluginContext.Log.InfoFormat("Order id  {0}", orderId);
            var data = context.GetRollbackData<CollectedDataDemoClass>();
            var slip = new ReceiptSlip
            {
                Doc = new XElement(Tags.Doc,
                    new XElement(Tags.Pair,
                        new XAttribute(Data.Cheques.Attributes.Left, "Return"),
                        new XAttribute(Data.Cheques.Attributes.Right, PaymentSystemKey),
                        new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                    new XElement(Tags.Pair,
                        new XAttribute(Data.Cheques.Attributes.Left, "Transaction ID"),
                        new XAttribute(Data.Cheques.Attributes.Right, transactionId.ToString()),
                        new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)),
                    new XElement(Tags.Pair,
                        new XAttribute(Data.Cheques.Attributes.Left, "was card"),
                        new XAttribute(Data.Cheques.Attributes.Right, data != null && data.IsCard ? "YES" : "NO"),
                        new XAttribute(Data.Cheques.Attributes.Fit, Data.Cheques.Attributes.Right)))
            };
            printer.Print(slip);

            // To abort any not successful payment action PaymentActionFailedException should be used.
            var success = true;
            if (!success)
                throw new PaymentActionFailedException("Fail to storno payment. This text will be shown in dialog window and storning operation will be aborted.");
        }

        public bool CanPaySilently(
            decimal sum,
            Guid? orderId,
            Guid paymentTypeId,
            IPaymentDataContext context)
        {
            PluginContext.Log.Info($"Checking of payment silent processing {sum}, оrder id {orderId}.");

            var customData = context.GetCustomData<PaymentAdditionalData>();
            return customData?.SilentPay ?? false;
        }

        public void OnPaymentDeleting([NotNull] IOrder order, [NotNull] IPaymentItem paymentItem, [NotNull] IUser cashier, [NotNull] IOperationService operationService, IReceiptPrinter printer,
            [NotNull] IViewManager viewManager, IPaymentDataContext context)
        {
            PluginContext.Log.Info($"External payment item {paymentItem.Id} is deleting, order id {order.Id}.");

            var showUnexpectedError = true;
            if (showUnexpectedError)
                throw new InvalidOperationException("A program operation error has occurred.");

            var showPaymentFailedError = false;
            if (showPaymentFailedError)
                throw new PaymentActionFailedException("Cancel deletion of a payment item. This text will be shown in dialog window.");

            var cancelPaymentDeleting = false;
            if (cancelPaymentDeleting)
            {
                viewManager.ShowErrorPopup("Deleting a payment item is handled by the SamplePaymentPlugin. This operation will be canceled.");
                throw new PaymentActionCancelledException();
            }

            //Order can now be modified via this method. For example we can add +1 to the amount of first object in order.
            var orderEditing = false;
            if (orderEditing)
            {
                var item = order.Items.OfType<IOrderCookingItem>().First();
                operationService.ChangeOrderCookingItemAmount(item.Amount + 1, item, order,
                    operationService.GetDefaultCredentials());
            }

        }

        private void CafeSessionClosing([NotNull] IReceiptPrinter printer)
        {
            PluginContext.Log.Info("Cafe session closing.");
            var slip = new ReceiptSlip
            {
                Doc = new XElement(Tags.Doc,
                    new XElement(Tags.Center, PaymentSystemKey),
                    new XElement(Tags.Center, "Cafe session closed."))
            };
            printer.Print(slip);
        }

        private static void CafeSessionOpening()
        {
            PluginContext.Log.Info("Cafe session opening.");
            const string message = "SamplePaymentPlugin: 'I can not connect to my server to open operation session. But I'll not stop openning iikoFront cafe session.'";
            PluginContext.Operations.AddNotificationMessage(message, "SamplePaymentPlugin");
        }

        private static void NavigatingToPaymentScreen(IOrder order, IOperationService operationService)
        {
            var donationItem = order.Donations.LastOrDefault();
            if (donationItem != null)
            {
                operationService.DeleteDonation(order, donationItem, PluginContext.Operations.GetDefaultCredentials());
            }
        }

        public void Dispose()
        {
            subscriptions?.Dispose();
        }
    }

    [Serializable]
    public class CollectedDataDemoClass
    {
        public bool IsCard;
        public string Data;
    }
}
