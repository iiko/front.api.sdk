using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Resto.Front.Api.Exceptions;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class LicensingTester : IDisposable
    {
        private const int ModuleId = 42;
        private Window window;
        private ListBox buttons;
        private readonly Dictionary<Button, IDisposable> slots = new Dictionary<Button, IDisposable>();

        private static readonly Guid[] ClientIds =
        {
            new Guid("B8A532AC-C5A4-4104-A67E-2727014F3748"),
            new Guid("6F19D8CC-AE00-4C5B-A4A4-48BB03EAB05D"),
            new Guid("CAD27E95-A877-49DA-98D0-C4DA03F48B6B"),                
            new Guid("E41809C1-8712-4648-B052-18DEE07849F0"),                
            new Guid("058ABF0F-393A-4034-BD73-494AA88F65BA"),                
            new Guid("D4A1DFF3-31FF-49B7-9F7E-3124D213F153"),                
            new Guid("3DF95EA6-BA40-4BB7-B66E-6BF1360C4F68"),                
            new Guid("D9B0134A-8DB7-4C31-B03A-D61C8983F011"),                
            new Guid("3A269331-F0F5-46D1-B0CC-781A217DBD13"),                
            new Guid("5ABB2970-2245-4285-B1D9-95B97ECD1042")                
        };

        public LicensingTester()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void EntryPoint()
        {
            buttons = new ListBox();
            window = new Window
            {
                Content = buttons,
                Width = 200,
                Height = 500,
                Topmost = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                ResizeMode = ResizeMode.CanMinimize,
                ShowInTaskbar = true,
                VerticalContentAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.SingleBorderWindow,
            };
            Enumerable.Range(1, ClientIds.Length).ForEach(AddClient);
            window.ShowDialog();
        }

        private void AddClient(int clientNumber)
        {
            var button = new Button { Content = string.Format("Connect {0}", clientNumber), Margin = new Thickness(2) };
            button.Click += (s, e) =>
            {
                IDisposable existingSlot;
                if (slots.TryGetValue(button, out existingSlot))
                {
                    existingSlot.Dispose();
                    slots.Remove(button);
                    button.Content = string.Format("Connect {0}", clientNumber);
                }
                else
                {
                    try
                    {
                        slots[button] = PluginContext.Licensing.AcquireSlot(ModuleId, ClientIds[clientNumber - 1]);
                        button.Content = string.Format("Disconnect {0}", clientNumber);
                    }
                    catch (InsufficientLicenseException ex)
                    {
                        MessageBox.Show(ex.Message, "Ooops");
                    }
                }
            };
            buttons.Items.Add(button);
        }

        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
            slots.Values.ForEach(slot => slot.Dispose());
        }
    }
}
