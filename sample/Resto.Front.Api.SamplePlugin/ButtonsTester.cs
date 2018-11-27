using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows;
using System.Xml.Linq;
using Resto.Front.Api.V5;
using Resto.Front.Api.V5.Data.Cheques;
using Resto.Front.Api.V5.UI;
using Resto.Front.Api.V5.Extensions;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class ButtonsTester : IDisposable
    {
        private readonly CompositeDisposable subscriptions;

        public ButtonsTester()
        {
            subscriptions = new CompositeDisposable
            {
                PluginContext.Integration.AddButton(new Button("SamplePlugin: Message Button",  (v, p, _) => MessageBox.Show("Message shown from Sample plugin."))),
                PluginContext.Integration.AddButton("SamplePlugin: Open iiko.ru", OpenIikoSite),
                PluginContext.Integration.AddButton("SamplePlugin: Print 'Test Printer'", Print),
                PluginContext.Integration.AddButton("SamplePlugin: Print 'Test List View'", ShowListPopup),
                PluginContext.Integration.AddButton("SamplePlugin: Print 'Test Keyboard View'", ShowKeyboardPopup)
            };
        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }

        private void Print(IViewManager viewManager, IReceiptPrinter receiptPrinter, IProgressBar progressBar)
        {
            receiptPrinter.Print(new ReceiptSlip
            {
                Doc = new XElement(Tags.Doc, new XElement(Tags.Center, "Test Printer"))
            });
        }

        private void OpenIikoSite(IViewManager viewManager, IReceiptPrinter receiptPrinter, IProgressBar progressBar)
        {
            using (var process = new Process
            {
                EnableRaisingEvents = false,
                StartInfo = { FileName = @"https://iiko.github.io/front.api.doc/" }
            })
            {
                process.Start();
            }

        }

        private void ShowListPopup(IViewManager viewManager, IReceiptPrinter receiptPrinter, IProgressBar progressBar)
        {
            var list = new List<string> { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet" };

            var selectedItem = list[2];
            var inputResult = viewManager.ShowChooserPopup("Select color", list, i => i, selectedItem, ButtonWidth.Narrower);
            PluginContext.Operations.AddNotificationMessage(
                inputResult == null
                    ? "Nothing"
                    : string.Format("Selected : {0}", inputResult),
                "SamplePlugin",
                TimeSpan.FromSeconds(15));
        }

        private void ShowKeyboardPopup(IViewManager viewManager, IReceiptPrinter receiptPrinter, IProgressBar progressBar)
        {
            var inputResult = viewManager.ShowKeyboard("Enter Name", isMultiline: false, capitalize: true);
            PluginContext.Operations.AddNotificationMessage(
                inputResult == null
                    ? "Nothing"
                    : string.Format("Entered : '{0}'", inputResult),
                "SamplePlugin",
                TimeSpan.FromSeconds(15));
        }

    }
}
