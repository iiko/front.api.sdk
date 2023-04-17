using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Resto.Front.Api.CustomerScreen.Settings;
using Resto.Front.Api.CustomerScreen.ViewModel;
using Timer = System.Timers.Timer;

namespace Resto.Front.Api.CustomerScreen.View.Controls
{
    /// <summary>
    /// Interaction logic for CustomerResultSumControl.xaml
    /// </summary>
    public sealed partial class CustomerResultSumControl
    {
        private readonly Timer timer = new Timer(5000) { AutoReset = false };
        private readonly TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public CustomerResultSumControl()
        {
            InitializeComponent();
            timer.Elapsed += Timer_OnResetChangedSum;
            var pathToLogo = CustomerScreenConfig.Instance.LogoImagePath;
            PluginContext.Log.InfoFormat("Path To Logo '{0}'", pathToLogo);
            try
            {
                if (!string.IsNullOrEmpty(pathToLogo) && File.Exists(pathToLogo))
                    imgLogo.Source = new BitmapImage(new Uri(Path.GetFullPath(pathToLogo)));
            }
            catch (Exception exp)
            {
                PluginContext.Log.ErrorFormat("Fail to load image from {0}. Error {1} ", pathToLogo, exp.Message);
            }
        }

        private void Timer_OnResetChangedSum(object sender, EventArgs e)
        {

            Task.Factory.StartNew(() =>
            {
                if (ChangeSumBlock != null)
                    ChangeSumBlock.Visibility = System.Windows.Visibility.Collapsed;
            }, CancellationToken.None, TaskCreationOptions.None, uiTaskScheduler);
        }

        public void ChangeSumChanged(decimal sum)
        {
            ((Order)DataContext).ChangeSum = sum;
            ChangeSumBlock.Visibility = System.Windows.Visibility.Visible;
            timer.Stop();
            timer.Start();
        }
    }
}
