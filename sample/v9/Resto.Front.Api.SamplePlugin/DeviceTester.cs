using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Device.Tasks;
using Button = System.Windows.Controls.Button;
using System.ComponentModel;
using System.Text;

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
        AddButton("Cash Register DirectIo ", CashRegisterDirectIo);
        AddButton("Get Document FiscalTags", GetDocumentFiscalTags);
        AddButton("Log Cash Register Data", LogCashRegisterData);
        AddButton("Log Cash Register Status", LogCashRegisterStatus);

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

    private static void CashRegisterDirectIo()
    {
        var cashRegister = GetDefaultCashRegister();

        var parameters = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
        var task = new Resto.Front.Api.Data.Device.Tasks.DirectIoTask("DirectIoWithInputOutputData", parameters,
            Guid.NewGuid(), "cashierName", Guid.NewGuid(), string.Empty);

        PluginContext.Log.Info($"Calling CashRegisterDirectIO(commandCode=\"DirectIoWithInputOutputData\")");
        var directIoResult = PluginContext.Operations.CashRegisterDirectIO(cashRegister, task, PluginContext.Operations.GetDefaultCredentials());
        PluginContext.Log.Info("CashRegisterDirectIO() completed");
        if (directIoResult.Document?.Markup != null)
            PluginContext.Log.Info($"  document = {directIoResult.Document?.Markup}");
        if (directIoResult.ResultData != null)
            foreach (var pair in directIoResult.ResultData)
                PluginContext.Log.Info($"  resultData: key=\"{pair.Key}\", value=\"{pair.Value}\"");
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

    private static void LogCashRegisterData()
    {
        var register = GetDefaultCashRegister();
        var data = PluginContext.Operations.GetCashRegisterData(register, PluginContext.Operations.GetDefaultCredentials());
        var sb = new StringBuilder();
        sb.AppendLine($"Current cash register: {register.Name}");
        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(data))
            sb.AppendLine($"{descriptor.Name} = {descriptor.GetValue(data) ?? "null"}");
        PluginContext.Log.Info(sb.ToString());
    }

    private static void LogCashRegisterStatus()
    {
        var register = GetDefaultCashRegister();
        var status = PluginContext.Operations.GetCashRegisterStatus(register, new[] { CashRegisterStatusField.NearPaperEnd, CashRegisterStatusField.SalesCount }, PluginContext.Operations.GetDefaultCredentials());
        var sb = new StringBuilder();
        sb.AppendLine($"Current cash register: {register.Name}");
        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(status))
            sb.AppendLine($"{descriptor.Name} = {descriptor.GetValue(status) ?? "null"}");
        PluginContext.Log.Info(sb.ToString());
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