using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Data.Brd;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class ClientView
    {
        private readonly ObservableCollection<IClient> clients = new ObservableCollection<IClient>();
        private string searchQuery;

        public ObservableCollection<IClient> Clients
        {
            get { return clients; }
        }

        public string SearchQuery
        {
            get { return searchQuery; }
            set
            {
                searchQuery = value;
                ReloadClients();
            }
        }

        public ClientView()
        {
            InitializeComponent();

            ReloadClients();
        }

        private void ReloadClients()
        {
            clients.Clear();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                PluginContext.Operations.SearchClients(searchQuery).ForEach(clients.Add);
            }
        }

        private void BtnRefreshClick(object sender, RoutedEventArgs e)
        {
            ReloadClients();
        }

        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            var windowAdd = new ClientBox(null)
                                {
                                    Title = GetType().Name
                                };
            windowAdd.ShowDialog();

            var createdClient = windowAdd.CreatedClient;
            if (createdClient == null)
                return;
            clients.Add(createdClient);
        }

        private void BtnChangeClick(object sender, RoutedEventArgs e)
        {
            var existingClient = (IClient)listBoxClient.SelectedItem;
            var windowChange = new ClientBox(existingClient)
                                   {
                                       Title = GetType().Name
                                   };
            var clientWasChanged = windowChange.ShowDialog();

            if (clientWasChanged == true)
            {
                clients.Remove(existingClient);
                var updatedClient = PluginContext.Operations.GetClientById(existingClient.Id);
                clients.Add(updatedClient);
            }
        }
    }
}
