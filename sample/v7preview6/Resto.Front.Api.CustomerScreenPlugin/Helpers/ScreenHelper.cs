using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Resto.Front.Api.CustomerScreen.Helpers
{
    internal sealed class ScreenHelper
    {
        private const int MonitorInfoPrimary = 1;

        private readonly List<MonitorInfo> monitors;

        public bool IsSecondMonitorExists { get; private set; }

        public int SecondMonitorTop { get; private set; }

        public int SecondMonitorLeft { get; private set; }

        public ScreenHelper()
        {
            monitors = new List<MonitorInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnum, IntPtr.Zero);

            monitors = monitors.OrderBy(m => m.Flags != MonitorInfoPrimary)
                 .ThenBy(m => !m.Monitor.IsEmpty())
                 .ThenBy(m => m.Monitor.Left)
                 .ThenBy(m => m.Monitor.Top)
                 .ToList();


            PluginContext.Log.InfoFormat("Found {0} monitors {1}", monitors.Count, string.Join("; ", monitors.Select(m => m.ToString()).ToArray()));

            if (monitors.Count < 2)
            {
                PluginContext.Log.Info("There is no second monitor");
                return;
            }

            var minLeft = monitors.Min(m => m.Monitor.Left);
            var minTop = monitors.Min(m => m.Monitor.Top);
            var maxRight = monitors.Max(m => m.Monitor.Right);
            var maxBottom = monitors.Max(m => m.Monitor.Bottom);

            var secondMonitor = monitors.Skip(1)
                .FirstOrDefault(m => m.Monitor.Left != minLeft || m.Monitor.Top != minTop || m.Monitor.Right != maxRight || m.Monitor.Bottom != maxBottom);

            if (secondMonitor.Monitor.IsEmpty())
            {
                PluginContext.Log.Info("Second monitor is empty");
                return;
            }

            IsSecondMonitorExists = true;
            SecondMonitorTop = secondMonitor.Monitor.Top;
            SecondMonitorLeft = secondMonitor.Monitor.Left;
        }

        #region MonitorsApi
        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public bool IsEmpty()
            {
                return Left == 0 && Top == 0 && Right == 0 && Bottom == 0;
            }

            public override string ToString()
            {
                return string.Format("[left={0}, top={1}, right={2}, bottom={3}]", Left, Top, Right, Bottom);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MonitorInfo
        {
            public uint Size;
            public Rect Monitor;
            public Rect Work;
            public uint Flags;

            public override string ToString()
            {
                return string.Format("\nsize = {0}; monitor = {1}; work = {2}; flags = {3}", Size, Monitor, Work, Flags);
            }
        }

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref  Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hmon, ref  MonitorInfo mi);

        private bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref  Rect lprcMonitor, IntPtr dwData)
        {
            var monitor = new MonitorInfo();
            monitor.Size = (uint)Marshal.SizeOf(monitor);
            GetMonitorInfo(hMonitor, ref  monitor);
            monitors.Add(monitor);
            return true;
        }
        #endregion

    }
}
