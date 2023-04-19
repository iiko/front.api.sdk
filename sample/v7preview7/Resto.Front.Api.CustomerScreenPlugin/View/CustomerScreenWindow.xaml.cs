using System;

namespace Resto.Front.Api.CustomerScreen.View
{
    /// <summary>
    /// Interaction logic for CustomerScreenWindow.xaml
    /// </summary>
    public partial class CustomerScreenWindow
    {
        public bool CanBeClosed = false;

        public CustomerScreenWindow()
        {
            InitializeComponent();
            SizeChanged += CustomerScreenWindow_SizeChanged;
            StateChanged += CustomerScreenWindow_StateChanged;
            Closing += CustomerScreenWindow_Closing;
        }

        void CustomerScreenWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PluginContext.Log.InfoFormat("Customer window try to close.");
            if (CanBeClosed)
            {
                PluginContext.Log.InfoFormat("Customer window closed.");
                return;
            }
            e.Cancel = true;
            PluginContext.Log.InfoFormat("Customer window closing aborted.");
        }

        private void CustomerScreenWindow_StateChanged(object sender, EventArgs e)
        {
            PluginContext.Log.InfoFormat("Customer window state changed. Window state is {0}", WindowState);
            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = System.Windows.WindowState.Maximized;
        }

        void CustomerScreenWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            PluginContext.Log.InfoFormat("Customer window size changed. Window state is {0}", WindowState);
            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = System.Windows.WindowState.Maximized;
        }

        public void ChangeSumChanged(decimal sum)
        {
            ctlResultSum.ChangeSumChanged(sum);
        }
    }
}
