using System;
using System.Windows;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    /// <summary>
    /// Interaction logic for ModifiersWindow.xaml
    /// </summary>
    public partial class ModifiersWindow : Window
    {
        private ModifiersWindowViewModel ViewModel { get; }

        public ModifiersWindow()
        {
            InitializeComponent();
            ViewModel = new ModifiersWindowViewModel();
            ViewModel.ShowError += ViewModel_ShowError;
            DataContext = ViewModel;
        }

        private void ViewModel_ShowError(string messageBoxText, string caption)
        {
            MessageBox.Show(this, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        private void ModifiersWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshNomenclatureCommand.Execute(null);
            ViewModel.RefreshLastOrderItemsCommand.Execute(null);
        }

        private void Tree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedNode = Tree.SelectedItem as TreeNodeViewModel;
        }

        private void ModifiersWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.ShowError -= ViewModel_ShowError;
        }
    }
}
