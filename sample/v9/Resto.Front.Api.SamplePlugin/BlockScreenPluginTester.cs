using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin
{
    using static PluginContext;

    /// <summary>
    /// <para>Является обёрткой над операциями плагина <b>BlockScreenPlugin</b>. Используемые операции:</para>
    /// <para><b>Initialize</b> - предварительная инициализация окна блокировки. Ускоряет первый показ окна. Вызов этой операции не является обязательным. Можно вызвать только для текущего терминала.</para>
    /// <para><b>BlockTerminal</b> - показать окно блокировки с заданным текстом. Можно вызвать только для текущего терминала.</para>
    /// <para><b>UnblockTerminal</b> - скрыть окно блокировки. Можно вызвать только для текущего терминала.</para>
    /// <para><b>GetTerminalIsBlocked</b> - проверка состояния блокировки. Можно вызвать для любого терминала.</para>
    /// </summary>
    /// <remarks>
    /// Для работы нужен установленный плагин <b>Resto.Front.Api.BlockScreenPlugin</b>
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    internal sealed class BlockScreenPluginTester : IDisposable
    {
        private const int ServiceModuleId = 21041518;
        private const string ServiceName = "BlockScreenPlugin";
        private const string Initialize = "Initialize";
        private const string GetTerminalIsBlocked = "GetTerminalIsBlocked";
        private const string BlockTerminal = "BlockTerminal";
        private const string UnblockTerminal = "UnblockTerminal";

        private readonly CompositeDisposable subscriptions;

        public BlockScreenPluginTester()
        {
            subscriptions = new CompositeDisposable
            {
                Operations.AddButtonToPluginsMenu("BlockScreen: Initialize", x => DoIfPluginOperationAvailiable(x.vm, InitializeBlockWindow)),
                Operations.AddButtonToPluginsMenu("BlockScreen: Show for 10 seconds", x => DoIfPluginOperationAvailiable(x.vm, ShowAndHideBlockWindow)),
                Operations.AddButtonToPluginsMenu("BlockScreen: Check state", x => DoIfPluginOperationAvailiable(x.vm, () => CheckBlockWindowState(x.vm)))
            };
        }

        /// <summary>
        /// Метод проверяет доступность плагина. Если плагин недоступен, то показывается окно ошибки. Если плагин доступен, то вызывается "action".
        /// </summary>
        private static void DoIfPluginOperationAvailiable(IViewManager viewManager, Action action)
        {
            if (!Operations.GetExternalOperations().Contains((ServiceModuleId, ServiceName, Initialize)))
            {
                viewManager.ShowErrorPopup($"External operation not found. The plugin \"{ServiceName}\" may not been installed.");
                return;
            }

            action();
        }

        /// <summary>
        /// Можно вызвать операцию Initialize для ускорения первого показа окна блокировки.
        /// </summary>
        private static void InitializeBlockWindow() => Operations.CallExternalOperation<object, bool>(ServiceModuleId, ServiceName, Initialize, Array.Empty<byte>());

        /// <summary>
        /// Показываем окно блокировки с текстом "Loading" на 10 секунд.
        /// </summary>
        private static void ShowAndHideBlockWindow()
        {
            Operations.CallExternalOperation<string, bool>(ServiceModuleId, ServiceName, BlockTerminal, "Loading");

            Task.Delay(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult(); //Показываем окно 10 секунд.

            Operations.CallExternalOperation<object, bool>(ServiceModuleId, ServiceName, UnblockTerminal, Array.Empty<byte>());
        }

        /// <summary>
        /// Проверяем состояние блокировки у всех терминалов.
        /// </summary>
        private static void CheckBlockWindowState(IViewManager viewManager)
        {
            var sb = new StringBuilder();
            foreach (var terminal in Operations.GetTerminals())
            {
                try
                {
                    var isBlocked = Operations.CallExternalOperation<byte[], bool>(ServiceModuleId, ServiceName, GetTerminalIsBlocked, Array.Empty<byte>(), terminal);
                    sb.AppendLine($"Terminal's \"{terminal.Name}\" blocking state: {isBlocked}");
                }
                catch (ExternalOperationCallingException e)
                {
                    sb.AppendLine($"Failed to get block state for terminal \"{terminal.Name}\". Error message: {e.Message}");
                }
            }

            viewManager.ShowOkPopup("Terminals' blocking state", sb.ToString());
        }

        public void Dispose() => subscriptions.Dispose();
    }
}
