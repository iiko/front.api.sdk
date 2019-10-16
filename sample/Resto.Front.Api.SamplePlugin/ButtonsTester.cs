using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Xml.Linq;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.UI;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin
{
    using static PluginContext;

    internal sealed class ButtonsTester : IDisposable
    {
        private readonly CompositeDisposable subscriptions;

        public ButtonsTester()
        {
            subscriptions = new CompositeDisposable
            {
                Integration.AddButton(new Button("SamplePlugin: Message Button",  (v, p) => MessageBox.Show("Message shown from Sample plugin."))),
                Integration.AddButton("SamplePlugin: Open iiko.ru", OpenIikoSite),
                Integration.AddButton("SamplePlugin: Print 'Test Printer'", Print),
                Integration.AddButton("SamplePlugin: Show 'Test List View'", ShowListPopup),
                Integration.AddButton("SamplePlugin: Show 'Test Keyboard View'", ShowKeyboardPopup),
                Integration.AddButton("SamplePlugin: Show input dialog", ShowInputDialog),
                Integration.AddButton("SamplePlugin: Show ok popup", ShowOkPopup),
                Integration.AddButton("SamplePlugin: Show Error popup", ShowErrorPopup),

                Integration.AddButtonOnClosedOrderView("SamplePlugin: Show ok popup", ShowOkPopupOnClosedOrderScreen),
                Integration.AddButtonOnPastOrderView("SamplePlugin: Show ok popup", ShowOkPopupOnPastOrderScreen),
            };
        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }

        private static void Print(IViewManager viewManager, IReceiptPrinter receiptPrinter)
        {
            receiptPrinter.Print(new ReceiptSlip
            {
                Doc = new XElement(Tags.Doc, new XElement(Tags.Center, "Test Printer"))
            });
        }

        private static void OpenIikoSite(IViewManager viewManager, IReceiptPrinter receiptPrinter)
        {
            using (var process = new Process
            {
                EnableRaisingEvents = false,
                StartInfo = { FileName = @"http://api.iiko.ru/" }
            })
            {
                process.Start();
            }
        }

        private static void ShowListPopup(IViewManager viewManager, IReceiptPrinter receiptPrinter)
        {
            var list = new List<string> { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet" };

            var selectedItem = list[2];
            var inputResult = viewManager.ShowChooserPopup("Select color", list, i => i, selectedItem, ButtonWidth.Narrower);
            ShowNotification(inputResult == null
                    ? "Nothing"
                    : $"Selected : {inputResult}");
        }

        private static void ShowKeyboardPopup(IViewManager viewManager, IReceiptPrinter receiptPrinter)
        {
            var inputResult = viewManager.ShowKeyboard("Enter Name", isMultiline: false, capitalize: true);
            ShowNotification(inputResult == null
                    ? "Nothing"
                    : $"Entered : '{inputResult}'");
        }

        private static void ShowInputDialog(IViewManager viewManager, IReceiptPrinter receiptPrinter)
        {
            var magicNumber = (NumberInputDialogResult)viewManager.ShowInputDialog("Enter your favourite number", InputDialogTypes.Number);
            if (magicNumber != null)
            {
                ShowNotification($"0x{magicNumber.Number:x}{Environment.NewLine}Did you know, how your favorite number looks in hex?");
                return;
            }

            var settings = new ExtendedInputDialogSettings
            {
                EnableBarcode = true,
                TabTitleBarcode = "Barcode",

                EnableCardSlider = true, // card sliding doesn't have UI, so it doesn't need a separate tab, so here is no title for that input type

                EnableNumericString = true,
                TabTitleNumericString = "Nasty number", // if user doesn't have a favourite number, probably, there is some number which he hates more than others?

                EnablePhone = true,
                TabTitlePhone = "Your mobile phone number"
            };
            var dialogResult = viewManager.ShowExtendedInputDialog("Opinion survey, step 2", "Try to enter something or scan barcode or swipe magnetic card or whatever.", settings);
            switch (dialogResult)
            {
                case null: // user cancelled the dialog
                    ShowNotification("Bye-bye!");
                    break;
                case BarcodeInputDialogResult barcode:
                    ShowNotification($"Barcode is: {barcode.Barcode}");
                    break;
                case CardInputDialogResult card:
                    ShowNotification($"Card number is: {card.Track2}");
                    break;
                case NumericStringInputDialogResult numericString:
                    // the entered number is string, it may be very long, parsing to int may cause an overflow, use ShowInputDialog+InputDialogTypes.Number for integer
                    ShowNotification(numericString.NumericString.Length == 1
                        ? "Hey, why not to try to enter a long number?"
                        : $"{new string(numericString.NumericString.Reverse().ToArray())}");
                    break;
                case PhoneInputDialogResult phone:
                    ShowNotification($"Now plugin could call to {phone.PhoneNumber}");
                    break;
                default:
                    throw new NotSupportedException(nameof(dialogResult.GetType));
            }
        }

        private static void ShowNotification(string message)
        {
            Operations.AddNotificationMessage(message, "SamplePlugin", TimeSpan.FromSeconds(3));
        }

        private void ShowOkPopup(IViewManager viewManager, IReceiptPrinter receiptPrinter)
        {
            viewManager.ShowOkPopup("Popup Title", "Message");
        }

        private void ShowErrorPopup(IViewManager viewManager, IReceiptPrinter receiptPrinter)
        {
            viewManager.ShowErrorPopup("Error Message");
        }

        private void ShowOkPopupOnClosedOrderScreen(IOrder closedOrder, ICashRegisterInfo cashRegister, IViewManager viewManager)
        {
            viewManager.ShowOkPopup("Popup Title", "Message shown from Sample plugin.");
        }

        private void ShowOkPopupOnPastOrderScreen(Guid pastOrderId, ICashRegisterInfo cashRegister, IViewManager viewManager)
        {
            viewManager.ShowOkPopup("Popup Title", "Message");
        }
    }
}
