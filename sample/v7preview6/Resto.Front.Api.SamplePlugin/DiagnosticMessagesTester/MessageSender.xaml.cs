using System;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.DiagnosticMessagesTester
{
    /// <summary>
    /// Interaction logic for MessageSender.xaml
    /// </summary>
    public sealed partial class MessageSender
    {
        public MessageSender()
        {
            InitializeComponent();

            Severity.Items.Add("Notification");
            Severity.Items.Add("Warning");
            Severity.Items.Add("Error");

            Severity.SelectedIndex = 0;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var expireTime = TimeSpan.FromSeconds(int.Parse(expire.Text));

            switch ((string)Severity.SelectedItem)
            {
                case "Notification":
                    if (expireTime == TimeSpan.Zero)
                        PluginContext.Operations.AddNotificationMessage(text.Text, "SamplePlugin");
                    else
                        PluginContext.Operations.AddNotificationMessage(text.Text, "SamplePlugin", expireTime);

                    break;

                case "Warning":
                    if (expireTime == TimeSpan.Zero)
                        PluginContext.Operations.AddWarningMessage(text.Text, "SamplePlugin");
                    else
                        PluginContext.Operations.AddWarningMessage(text.Text, "SamplePlugin", expireTime);

                    break;

                case "Error":
                    if (expireTime == TimeSpan.Zero)
                        PluginContext.Operations.AddErrorMessage(text.Text, "SamplePlugin");
                    else
                        PluginContext.Operations.AddErrorMessage(text.Text, "SamplePlugin", expireTime);

                    break;
            }
        }
    }
}
