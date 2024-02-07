using System;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.CustomData
{
    /// <summary>
    /// Interaction logic for ModifiersWindow.xaml
    /// </summary>
    public partial class CustomDataWindow : Window
    {
        private CustomDataWindowViewModel ViewModel { get; }

        public CustomDataWindow()
        {
            InitializeComponent();
            ViewModel = new CustomDataWindowViewModel();
            ViewModel.ShowMessage += ViewModel_ShowMessage;
            DataContext = ViewModel;
        }

        private void ViewModel_ShowMessage(string message)
        {
            MessageBox.Show(this, message);
        }

        private void CustomDataWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.ShowMessage -= ViewModel_ShowMessage;
            ViewModel.Dispose();
        }
    }
}
