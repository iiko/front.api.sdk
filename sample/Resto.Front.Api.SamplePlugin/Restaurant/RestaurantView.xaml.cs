using System.Windows;
using Resto.Front.Api.V5;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class RestaurantView
    {
        public RestaurantView()
        {
            InitializeComponent();

            ReloadRestaurant();
        }

        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            ReloadRestaurant();
        }

        private void ReloadRestaurant()
        {
            var restaurant = PluginContext.Operations.GetHostRestaurant();
            var sections = PluginContext.Operations.GetRestaurantSections();
            var users = PluginContext.Operations.GetUsers();
            treeItemRestaurant.DataContext = restaurant;
            treeItemSections.ItemsSource = sections;
            treeItemUsers.ItemsSource = users;
        }
    }
}
