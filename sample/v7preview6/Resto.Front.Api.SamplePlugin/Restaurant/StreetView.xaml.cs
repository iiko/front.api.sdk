using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Extensions;
using MessageBox = System.Windows.MessageBox;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class StreetView
    {
        public ObservableCollection<IStreet> Streets { get; } = new ObservableCollection<IStreet>();

        public StreetView()
        {
            InitializeComponent();

            SearchStreets();
        }

        private void SearchStreets()
        {
            Streets.Clear();
            PluginContext.Operations.SearchStreets(txtSearch.Text).ForEach(Streets.Add);
        }

        private void BtnSearchClick(object sender, RoutedEventArgs e)
        {
            SearchStreets();
        }

        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            var windowAdd = new StreetBox
            {
                Title = GetType().Name
            };
            windowAdd.ShowDialog();
            var createdStreet = windowAdd.CreatedStreet;
            if (createdStreet == null)
                return;
            Streets.Add(createdStreet);
        }

        private void BtnRenameClick(object sender, RoutedEventArgs e)
        {
            var street = (IStreet)listBoxStreet.SelectedItem;

            if (string.IsNullOrEmpty(txtBox.Text) || txtBox.Text == street.Name)
            {
                MessageBox.Show(string.Format("Enter new name for {0} street.", street.Name));
                return;
            }

            PluginContext.Operations.ChangeStreetName(txtBox.Text, street, PluginContext.Operations.GetCredentials());

            var changedStreet = PluginContext.Operations.GetStreetById(street.Id);
            Streets.Remove(street);
            Streets.Add(changedStreet);

            listBoxStreet.SelectedItem = changedStreet;
            txtBox.Clear();
        }

        private void BtnDeleteClick(object sender, RoutedEventArgs e)
        {
            var street = (IStreet)listBoxStreet.SelectedItem;

            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.DeleteOrRestoreStreet(true, street);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);

            Streets.Remove(street);
            var updatedStreet = PluginContext.Operations.GetStreetById(street.Id);
            Streets.Add(updatedStreet);

            listBoxStreet.SelectedItem = updatedStreet;
        }

        private void ListBoxStreetSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            txtBox.SelectAll();
        }

        private void BtnUndeleteClick(object sender, RoutedEventArgs e)
        {
            var street = (IStreet)listBoxStreet.SelectedItem;

            PluginContext.Operations.DeleteOrRestoreStreet(false, street, PluginContext.Operations.GetCredentials());

            Streets.Remove(street);
            var updatedStreet = PluginContext.Operations.GetStreetById(street.Id);
            Streets.Add(updatedStreet);

            listBoxStreet.SelectedItem = updatedStreet;
        }
    }
}
