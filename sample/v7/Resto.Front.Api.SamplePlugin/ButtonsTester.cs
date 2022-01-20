using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Xml.Linq;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.UI;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin
{
    using static PluginContext;

    internal sealed class ButtonsTester : IDisposable
    {
        private const string IikoIcon = "M 107.81,45.16 C  104.46,45.16 104.39,40.82 104.39,37.49 104.39,34.14 104.46,29.81 107.81,29.81 111.16,29.81 111.21,34.14 111.21,37.49 111.21,40.82 111.16,45.16 107.81,45.16 z M 107.81,20.5 C 95.48,20.5 87.69,26.89 87.69,37.5 87.69,48.07 95.48,54.46 107.81,54.46 120.13,54.46 127.94,48.07 127.94,37.49 127.94,26.89 120.13,20.5 107.81,20.5 z M 14.32,22 L 1.3,22 C 0.58,22 0,22.58 0,23.3 L 0,53.16 C 0,53.88 0.58,54.46 1.3,54.46 L 14.32,54.46 C 15.04,54.46 15.62,53.88 15.62,53.16 L 15.62,23.3 C 15.62,22.59 15.04,22 14.32,22 z M 14.32,7.76 L 1.3,7.76 C 0.58,7.76 0,8.34 0,9.06 L 0,17.31 C 0,18.02 0.58,18.61 1.3,18.61 L 14.32,18.61 C 15.04,18.61 15.62,18.02 15.62,17.31 L 15.62,9.05 C 15.62,8.34 15.04,7.75 14.32,7.75 z M 36.12,22 L 23.1,22 C 22.39,22 21.8,22.58 21.8,23.3 L 21.8,53.16 C 21.8,53.88 22.39,54.46 23.1,54.46 L 36.13,54.46 C 36.84,54.46 37.43,53.88 37.43,53.16 L 37.43,23.3 C 37.43,22.59 36.84,22 36.13,22 z M 36.12,7.76 L 23.1,7.76 C 22.39,7.76 21.8,8.34 21.8,9.06 L 21.8,17.31 C 21.8,18.02 22.39,18.61 23.1,18.61 L 36.13,18.61 C 36.84,18.61 37.43,18.02 37.43,17.31 L 37.43,9.05 C 37.43,8.34 36.84,7.75 36.13,7.75 z M 75.47,37.08 A 1.3,1.3,0,0,1,75.54,35.6 L 84.36,24.08 A 1.3,1.3,0,0,0,83.33,22 L 69,22 C 68.57,22 68.17,22.21 67.93,22.56 L 59.47,34.9 59.21,34.9 59.21,9.05 C 59.21,8.34 58.63,7.75 57.91,7.75 L 44.91,7.75 C 44.2,7.75 43.61,8.34 43.61,9.05 L 43.61,53.16 C 43.61,53.88 44.2,54.46 44.91,54.46 L 57.91,54.46 C 58.63,54.46 59.21,53.88 59.21,53.16 L 59.21,38.72 59.47,38.72 67.83,53.79 C 68.06,54.2 68.49,54.46 68.96,54.46 L 84.05,54.46 A 1.3,1.3,0,0,0,85.15,52.47 z M 134.27,20.5 L 133.77,21.99 133.29,23.47 132.32,20.5 131.49,20.5 131.49,24.33 132.06,24.33 132.06,21.46 133.03,24.33 133.55,24.33 C 133.72,23.87 133.87,23.39 134.03,22.9 L 134.52,21.46 134.52,24.33 135.09,24.33 135.09,20.5 z M 127.94,21.05 L 129.16,21.05 129.16,24.33 129.76,24.33 129.76,21.05 130.99,21.05 130.99,20.5 127.94,20.5 z M 112.14,15.32 L 112.03,15.49 A 3.76,3.76,0,0,0,111.55,13.54 A 10,10,0,0,0,109.62,11.2 C 108.58,10.26 107.57,9.52 106.99,8.17 106.85,7.78 106.73,7.27 106.7,6.87 106.68,6.36 106.77,5.93 106.97,5.47 107.09,5.17 107.25,4.89 107.41,4.63 107.4,4.89 107.41,5.16 107.45,5.42 107.62,6.38 108,7.3 108.61,8.09 109.26,8.96 110.15,9.59 110.94,10.33 111.86,11.2 112.82,12.36 112.75,13.68 A 3.4,3.4,0,0,1,112.15,15.32 z M 108.36,15.32 L 108.23,15.52 A 3.79,3.79,0,0,0,107.74,13.55 A 9.41,9.41,0,0,0,105.81,11.2 C 104.8,10.26 103.76,9.52 103.21,8.17 A 3.51,3.51,0,0,1,103.18,5.47 C 103.3,5.17 103.45,4.9 103.61,4.64 103.6,4.9 103.6,5.16 103.64,5.42 103.81,6.38 104.22,7.3 104.8,8.09 105.47,8.96 106.34,9.59 107.14,10.33 108.05,11.2 109.02,12.36 108.94,13.68 108.92,14.28 108.68,14.82 108.36,15.32 z M 115.79,14.87 A 3.77,3.77,0,0,0,115.33,13.54 A 9.4,9.4,0,0,0,113.4,11.2 C 112.39,10.26 111.35,9.52 110.8,8.17 110.63,7.78 110.5,7.27 110.5,6.87 110.49,6.36 110.58,5.93 110.77,5.47 111.17,4.5 111.77,3.77 112.5,3.12 113.09,2.59 113.09,1.67 112.51,1.13 L 111.71,0.37 C 111.21,-0.1 110.42,-0.12 109.89,0.33 A 9.5,9.5,0,0,0,108.94,1.25 L 108.89,1.31 108.73,1.12 107.92,0.37 A 1.36,1.36,0,0,0,106.12,0.32 C 105.77,0.6 105.44,0.91 105.14,1.25 L 105.08,1.31 104.92,1.13 104.12,0.37 C 103.62,-0.1 102.83,-0.12 102.3,0.33 A 9.5,9.5,0,0,0,101.35,1.25 C 100.36,2.33 99.59,3.9 99.85,5.42 100.03,6.38 100.41,7.3 101.01,8.09 101.66,8.96 102.56,9.59 103.35,10.33 104.27,11.2 105.23,12.36 105.16,13.68 105.11,14.28 104.9,14.82 104.56,15.32 104.1,16.02 103.56,16.62 102.98,17.18 102.43,17.73 102.44,18.62 103.01,19.15 L 103.85,19.92 C 104.38,20.42 105.21,20.4 105.72,19.89 106.03,19.59 106.33,19.26 106.62,18.94 L 106.81,19.14 107.65,19.92 C 108.19,20.42 109.01,20.39 109.53,19.88 L 110.43,18.94 C 110.48,19.01 110.53,19.08 110.6,19.14 L 111.44,19.91 C 111.97,20.41 112.8,20.39 113.32,19.88 113.82,19.38 114.32,18.86 114.75,18.31 115.18,17.76 115.5,17.08 115.69,16.43 A 3.8,3.8,0,0,0,115.79,14.87 z M -7.37,47.344 z";
        private const string TriangleIcon = "M 0.00,10.0 L 10.0,20.0 20.0,0.00";
        private readonly CompositeDisposable subscriptions;

        public ButtonsTester()
        {
            subscriptions = new CompositeDisposable
            {
                Operations.AddButtonToPluginsMenu("SamplePlugin: Message button", x => x.vm.ShowOkPopup("Sample", "Message shown from Sample plugin.")),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Open iiko.ru", _ => OpenIikoSite()),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Test receipt printer", x => Print(x.printer)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show list view", x => ShowListPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show list with quantities view", x => ShowListWithQuantitiesPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show keyboard view", x => ShowKeyboardPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show extended keyboard view", x => ShowExtendedKeyboardPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show input dialog", x => ShowInputDialog(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show ok popup", x => ShowOkPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show error popup", x => ShowErrorPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show date numpad popup", x => ShowDateNumpadPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show calendar popup", x => ShowCalendarPopup(x.vm)),
                Operations.AddButtonToPluginsMenu("SamplePlugin: Show date and time popup", x => ShowDateTimePopup(x.vm)),

                Operations.AddButtonToClosedOrderScreen("SamplePlugin: Show ok popup", x => ShowOkPopupOnClosedOrderScreen(x.vm), TriangleIcon),
                Operations.AddButtonToProductsReturnScreen("SamplePlugin: Show ok popup", x => ShowOkPopupOnProductsReturnScreen(x.vm)),
                Operations.AddButtonToOrderEditScreen("Sample Button", x =>
                {
                    var guest = x.order.Guests.First();
                    if (!x.vm.ShowYesNoPopup("Sample Plugin", $"Do you want to rename the first guest “{guest.Name}”?"))
                        return;

                    x.vm.ChangeProgressBarMessage("Renaming guest...");
                    var newName = x.vm.ShowKeyboard("Enter new name for the guest", guest.Name, false, 255);
                    if (string.IsNullOrEmpty(newName))
                        return;

                    x.os.RenameOrderGuest(x.order.Guests.First().Id, newName, x.order, x.os.GetCredentials());
                }, IikoIcon),
                Operations.AddButtonToPaymentScreen("Sample Plugin", false, true, x =>
                {
                    x.vm.ShowOkPopup("Sample plugin", "Payment screen click on sample button");
                    var isChecked = !x.state.isChecked;
                    var caption = isChecked ? "Sample Checked" : "Sample Plugin";
                    x.os.UpdatePaymentScreenButtonState(x.state.buttonId, caption, isChecked);
                }, IikoIcon).buttonRegistration,
            };
            

        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }

        private static void Print(IReceiptPrinter receiptPrinter)
        {
            receiptPrinter.Print(new ReceiptSlip
            {
                Doc = new XElement(Tags.Doc, new XElement(Tags.Center, "Test Printer"))
            });
        }

        private static void OpenIikoSite()
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

        private static void ShowListPopup(IViewManager viewManager)
        {
            var list = new List<string> { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet" };

            var selectedItem = list[2];
            var inputResult = viewManager.ShowChooserPopup("Select color", list, i => i, selectedItem, ButtonWidth.Narrower);
            ShowNotification(inputResult == null
                    ? "Nothing"
                    : $"Selected : {inputResult}");
        }

        private static void ShowListWithQuantitiesPopup(IViewManager viewManager)
        {
            const string title = "Santa's gift";
            const string hintText = "Choose sweets";
            var list = new List<(string name, int quantity, int minimalQuantity, int maximalQuantity)>
            {
                ("Chocolate",   0, 0, 3),
                ("Tangerines",  0, 0, 3),
                ("Candies",     0, 0, 3),
                ("Waffles",     0, 0, 3),
                ("Biscuits",    0, 0, 3),
                ("Marshmallow", 0, 0, 3),
                ("Meringue",    0, 0, 3),
            };

            var inputResult = viewManager.ShowQuantityChangerPopup(title, hintText, 0, 12, list);
            ShowNotification(inputResult == null
                    ? "Nothing"
                    : $"Selected : {string.Join(", ", inputResult.Zip(list, (quantity, item) => (name: item.name, quantity: quantity)).Where(i => i.quantity != 0).Select(i => $"{i.quantity} × {i.name}"))}",
                TimeSpan.FromSeconds(10));
        }

        private static void ShowKeyboardPopup(IViewManager viewManager) 
        {
            var inputResult = viewManager.ShowKeyboard("Enter Name", isMultiline: false, capitalize: true);
            ShowNotification(inputResult == null
                    ? "Nothing"
                    : $"Entered : '{inputResult}'");
        }

        static string GetExtendedKeyboardPopupResult(IInputDialogResult inputResult) 
        {
            switch(inputResult) {
                case StringInputDialogResult str: return $"Entered  Text: '{str.Result}'";
                case CardInputDialogResult card: return $"Entered Card: '{card.FullCardTrack}'";
                case BarcodeInputDialogResult barcode: return $"Scanned Barcode: '{barcode.Barcode}'";
                default: return "Nothing";
            }
        }

        private static void ShowExtendedKeyboardPopup(IViewManager viewManager)
        {
            var inputResult = viewManager.ShowExtendedKeyboardDialog("Keyboard with Barcode And Card Slider", enableBarcode: true, enableCardSlider: true);
            ShowNotification(GetExtendedKeyboardPopupResult(inputResult));
        }

        private static void ShowInputDialog(IViewManager viewManager)
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
                case StringInputDialogResult str:
                    // the entered number is string, it may be very long, parsing to int may cause an overflow, use ShowInputDialog+InputDialogTypes.Number for integer
                    ShowNotification(str.Result.Length == 1
                        ? "Hey, why not to try to enter a long number?"
                        : $"{new string(str.Result.Reverse().ToArray())}");
                    break;
                case PhoneInputDialogResult phone:
                    ShowNotification($"Now plugin could call to {phone.PhoneNumber}");
                    break;
                default:
                    throw new NotSupportedException(nameof(dialogResult.GetType));
            }
        }

        private static void ShowNotification(string message, TimeSpan? timeout = null)
        {
            Operations.AddNotificationMessage(message, "SamplePlugin", timeout ?? TimeSpan.FromSeconds(3));
        }

        private void ShowOkPopup(IViewManager viewManager)
        {
            viewManager.ShowOkPopup("Popup Title", "Message");
        }

        private void ShowErrorPopup(IViewManager viewManager)
        {
            viewManager.ShowErrorPopup("Error Message");
        }

        private void ShowDateNumpadPopup(IViewManager viewManager)
        {
            var selectedDate = viewManager.ShowDateNumpadPopup(DateTime.Today, "Popup Title");
            if (selectedDate != null)
            {
                ShowNotification($"Selected date: {selectedDate:d}");
            }
        }

        private void ShowCalendarPopup(IViewManager viewManager)
        {
            var selectedDate = viewManager.ShowCalendarPopup(DateTime.Today, "Popup Title", DateTime.Today, DateTime.Today.AddYears(1));
            if (selectedDate != null)
            {
                ShowNotification($"Selected date: {selectedDate:d}");
            }
        }

        private void ShowDateTimePopup(IViewManager viewManager)
        {
            var selectedDateTime = viewManager.ShowDateTimePopup(DateTime.Now, "Popup Title", DateTime.Today, DateTime.Today.AddYears(1));
            if (selectedDateTime != null)
            {
                ShowNotification($"Selected date and time: {selectedDateTime}");
            }
        }

        private void ShowOkPopupOnClosedOrderScreen(IViewManager viewManager)
        {
            viewManager.ShowOkPopup("Popup Title", "Message shown from Sample plugin.");
        }

        private void ShowOkPopupOnProductsReturnScreen(IViewManager viewManager)
        {
            viewManager.ShowOkPopup("Popup Title", "Message");
        }
    }
}
