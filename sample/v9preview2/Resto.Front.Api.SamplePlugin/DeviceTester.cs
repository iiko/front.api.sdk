using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Device.Tasks;
using Button = System.Windows.Controls.Button;

namespace Resto.Front.Api.SamplePlugin;

internal sealed class DeviceTester : IDisposable
{
    private Window window;
    private ItemsControl buttons;

    public DeviceTester()
    {
        var windowThread = new Thread(EntryPoint);
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();
    }

    private void EntryPoint()
    {
        buttons = new ItemsControl();
        window = new Window
        {
            Title = "Device tester plugin",
            Content = new ScrollViewer
            {
                Content = buttons
            },
            Width = 200,
            Height = 500,
            Topmost = true,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            ResizeMode = ResizeMode.CanMinimize,
            ShowInTaskbar = true,
            VerticalContentAlignment = VerticalAlignment.Center,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            WindowStyle = WindowStyle.SingleBorderWindow,
            SizeToContent = SizeToContent.Width
        };

        AddButton("Start Cash Register", CashRegisterStart);
        AddButton("Stop Cash Register", CashRegisterStop);
        AddButton("Get Document FiscalTags", GetDocumentFiscalTags);

        window.ShowDialog();
    }


    private void AddButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(2),
            Padding = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        button.Click += (s, e) =>
        {
            try
            {
                action();
                MessageBox.Show("Operation completed. See plugin log for details.", "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", text, Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };
        buttons.Items.Add(button);
    }


    private static void CashRegisterStart()
    {
        var cashRegister = GetDefaultCashRegister();
        PluginContext.Log.Info("Calling CashRegisterStart()");
        PluginContext.Operations.CashRegisterStart(cashRegister, PluginContext.Operations.GetDefaultCredentials());
        PluginContext.Log.Info("CashRegisterStart() completed");
    }

    private static void CashRegisterStop()
    {
        var cashRegister = GetDefaultCashRegister();
        PluginContext.Log.Info("Calling CashRegisterStop()");
        PluginContext.Operations.CashRegisterStop(cashRegister, PluginContext.Operations.GetDefaultCredentials());
        PluginContext.Log.Info("CashRegisterStop() completed");
    }

    private static void GetDocumentFiscalTags()
    {
        var cashRegister = GetDefaultCashRegister();
        const int fdNumber = 674;

        var task = new GetFiscalTagsTask(fdNumber, Guid.NewGuid(), "Admin", Guid.NewGuid(), string.Empty);
        PluginContext.Log.Info($"Calling GetDocumentFiscalTags(fdNumber={fdNumber})");
        var fiscalTagsResult = PluginContext.Operations.GetDocumentFiscalTags(cashRegister, task, PluginContext.Operations.GetDefaultCredentials());
        PluginContext.Log.Info("GetDocumentFiscalTags() completed");
        PluginContext.Log.Info($"DocumentType={fiscalTagsResult.DocumentType}, total tags={fiscalTagsResult.Tags.Count}. Detailed tag info:");
        foreach (var tag in fiscalTagsResult.Tags)
            LogTagInfo(tag, 1);
    }

    private static void LogTagInfo(FiscalTag tag, int level)
    {
        var levelPrefix = new string(' ', level * 2);
        if (tag.ChildTags.IsEmpty())
        {
            PluginContext.Log.Info($"{levelPrefix}{tag.Code}: {tag.Value}");
        }
        else
        {
            PluginContext.Log.Info($"{levelPrefix}{tag.Code}:");
            foreach (var childTag in tag.ChildTags)
                LogTagInfo(childTag, level + 1);
        }
    }

    private static ICashRegisterInfo GetDefaultCashRegister()
    {
        var cashRegister = PluginContext.Operations.GetHostTerminalPointsOfSale().FirstOrDefault()?.CashRegister;

        if (cashRegister == null)
            PluginContext.Log.Warn("Default cash register not found.");
        return cashRegister;
    }

    public void Dispose()
    {
        window.Dispatcher.InvokeShutdown();
        window.Dispatcher.Thread.Join();
    }
}