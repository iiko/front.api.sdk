using System.Windows.Threading;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal static class DispatcherHelper
    {
        public static Dispatcher UiDispatcher { get; private set; }

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
}
