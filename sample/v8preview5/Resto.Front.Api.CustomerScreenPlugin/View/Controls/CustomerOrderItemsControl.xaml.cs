using System.Collections.Specialized;

namespace Resto.Front.Api.CustomerScreen.View.Controls
{
    /// <summary>
    /// Interaction logic for CustomerOrderItemsControl.xaml
    /// </summary>
    public partial class CustomerOrderItemsControl
    {
        public CustomerOrderItemsControl()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)lstOrder.Items).CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (lstOrder.Items.Count > 0)
                svItems.ScrollToEnd();
        }
    }

}
