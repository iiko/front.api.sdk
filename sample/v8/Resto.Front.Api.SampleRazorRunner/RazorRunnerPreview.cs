using System.Reactive.Disposables;
using System.Threading;
using System.Windows.Threading;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SampleRazorRunner;

/// <summary>
/// Тестовый плагин для демонстрации возможности использования синтаксиса Razor для формирования чеков в Api.
/// </summary>
[UsedImplicitly]
[PluginLicenseModuleId(21005108)]
internal sealed class RazorRunnerPreview : IFrontPlugin
{
    private readonly CompositeDisposable resources = new();
    private readonly object syncObject = new();
    private bool disposed;
    public RazorRunnerPreview()
    {
        var windowThread = new Thread(EntryPoint);
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();
    }

    private void EntryPoint()
    {
        RazorRunnerPreviewWindow razorRunnerPreviewWindow;
        lock (syncObject)
        {
            if (disposed)
                return;

            DispatcherHelper.Initialize();
            razorRunnerPreviewWindow = new RazorRunnerPreviewWindow();

            resources.Add(Disposable.Create(() =>
            {
                razorRunnerPreviewWindow.Dispatcher.InvokeShutdown();
                razorRunnerPreviewWindow.Dispatcher.Thread.Join();
                DispatcherHelper.Reset();
            }));
        }

        var logger = PluginContext.Log;
        logger.Info("Show RazorRunnerPreview dialog...");
        razorRunnerPreviewWindow.ShowDialog();
        logger.Info("Closed RazorRunnerPreview dialog...");
    }

    public void Dispose()
    {
        if (disposed)
            return;

        lock (syncObject)
        {
            resources.Dispose();
            PluginContext.Log.Info("RazorRunnerPreview stopped");
            disposed = true;
        }
    }
}

internal static class DispatcherHelper
{
    private static Dispatcher UiDispatcher { get; set; }

    public static void Initialize()
    {
        if (UiDispatcher is null || !UiDispatcher.Thread.IsAlive)
            UiDispatcher = Dispatcher.CurrentDispatcher;
    }

    public static void Reset()
    {
        UiDispatcher = null;
    }
}