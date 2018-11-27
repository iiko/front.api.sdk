using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api.V5.Data.Brd;
using Resto.Front.Api.V5.Extensions;
using Resto.Front.Api.V5;
using MessageBox = System.Windows.MessageBox;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class StreetView
    {
        private readonly ObservableCollection<IStreet> streets = new ObservableCollection<IStreet>();

        public ObservableCollection<IStreet> Streets
        {
            get { return streets; }
        }

        public StreetView()
        {
            InitializeComponent();

            ReloadStreet();
        }

        private void ReloadStreet()
        {
            streets.Clear();
            PluginContext.Operations.GetAllStreets().ForEach(streets.Add);
        }

        private void BtnRefreshClick(object sender, RoutedEventArgs e)
        {
            ReloadStreet();
        }

        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            var windowAdd = new StreetBox()
                                   {
                                       Title = GetType().Name
                                   };
            windowAdd.ShowDialog();
            var createdStreet = windowAdd.CreatedStreet;
            if (createdStreet == null)
                return;
            streets.Add(createdStreet);
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
            streets.Remove(street);
            streets.Add(changedStreet);

            listBoxStreet.SelectedItem = changedStreet;
            txtBox.Clear();
        }

        private void BtnDeleteClick(object sender, RoutedEventArgs e)
        {
            var street = (IStreet)listBoxStreet.SelectedItem;

            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.DeleteOrRestoreStreet(true, street);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);

            streets.Remove(street);
            var updatedStreet = PluginContext.Operations.GetStreetById(street.Id);
            streets.Add(updatedStreet);

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

            streets.Remove(street);
            var updatedStreet = PluginContext.Operations.GetStreetById(street.Id);
            streets.Add(updatedStreet);

            listBoxStreet.SelectedItem = updatedStreet;
        }
    }
}
