using System;
using System.Windows;
using System.Windows.Controls;
using Resto.Front.Api.Data.Kitchen;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    public sealed partial class KitchenLoadMonitoringView
    {
        public KitchenLoadMonitoringView()
        {
            InitializeComponent();
        }

        private void ChangeStatus_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var viewModel = (CookingProductModel)menuItem.DataContext;
            var newStatus = (KitchenOrderItemProcessingStatus)menuItem.Tag;

            try
            {
                var result = PluginContext.Operations.ChangeKitchenOrderItemsProcessingStatus(viewModel.Order, new[] { viewModel.CookingItem }, Array.Empty<IKitchenOrderModifierItem>(), newStatus);
                if (!result)
                    MessageBox.Show("Operation failed, try again.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
