using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class BanquetAndReserveView
    {
        public ObservableCollection<IPaymentItem> PaymentItems { get; } = new ObservableCollection<IPaymentItem>();

        private readonly PhoneDto primaryPhone = new PhoneDto
        {
            PhoneValue = "+79991112233",
            IsMain = true
        };

        private readonly PhoneDto secondaryPhone = new PhoneDto
        {
            PhoneValue = "+79991114455",
            IsMain = false
        };

        public BanquetAndReserveView()
        {
            InitializeComponent();
        }

        private void btnCreateBanquet_Click(object sender, RoutedEventArgs e)
        {
            var editSession = PluginContext.Operations.CreateEditSession();

            var client = editSession.CreateClient(Guid.NewGuid(), "Jonathan", new List<PhoneDto> { primaryPhone, secondaryPhone }, null, DateTime.Now);
            var table = PluginContext.Operations.GetTables().FirstOrDefault();
            var order = editSession.CreateOrder(table);
            var guest = editSession.AddOrderGuest("Jonathan", order);
            var orderItemProduct = PluginContext.Operations.GetHierarchicalMenu().Products.First();
            editSession.AddOrderProductItem(1m, orderItemProduct, order, guest, null);
            editSession.ChangeEstimatedOrderGuestsCount(2, order);

            editSession.CreateBanquet(DateTime.Now + TimeSpan.FromHours(2), client, order);
            SubmitChanges(editSession);
        }

        private static void SubmitChanges([NotNull] IEditSession editSession)
        {
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
        }

        private void btnStartBanquet_Click(object sender, RoutedEventArgs e)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var user = PluginContext.Operations.GetUser(credentials);
            var createdBanquet = PluginContext.Operations.GetReserves().LastOrDefault(b => b.Status == ReserveStatus.New);

            if (createdBanquet != null)
                try
                {
                    PluginContext.Operations.StartBanquet(createdBanquet, user, credentials);
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", "Start Banquet", Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
                MessageBox.Show("Banquet wasn't found!");
        }

        private void btnShowOrderInfo_Click(object sender, RoutedEventArgs e)
        {
            var createdBanquet = PluginContext.Operations.GetReserves().LastOrDefault();
            if (createdBanquet != null)
            {
                if (createdBanquet.Order != null)
                {
                    var order = PluginContext.Operations.GetOrderById(createdBanquet.Order.Id);
                    PaymentItems.Clear();
                    order.Payments.ForEach(PaymentItems.Add);
                }
                else
                {
                    MessageBox.Show("Order is empty!");
                }
            }
            else
            {
                MessageBox.Show("Banquet wasn't found!");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            PaymentItems.Clear();
        }

        private void btnCancelBanquet_Click(object sender, RoutedEventArgs e)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var createdBanquet = PluginContext.Operations.GetReserves().LastOrDefault(b => b.Status == ReserveStatus.New);

            if (createdBanquet != null)
                try
                {
                    PluginContext.Operations.CancelReserve(credentials, createdBanquet, ReserveCancelReason.ClientRefused);
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", "Cancel Banquet", Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
                MessageBox.Show("Banquet wasn't found!");
        }

        private void btnPrintBanquet_Click(object sender, RoutedEventArgs e)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var createdBanquet = PluginContext.Operations.GetReserves().LastOrDefault();

            if (createdBanquet != null && createdBanquet.Status == ReserveStatus.New && createdBanquet.Order != null)
                try
                {
                    PluginContext.Operations.PrintBanquet(credentials, createdBanquet);
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", "Print Banquet", Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
                MessageBox.Show("Banquet wasn't found!");
        }

        private void btnCreateReserve_Click(object sender, RoutedEventArgs e)
        {
            var editSession = PluginContext.Operations.CreateEditSession();

            var client = editSession.CreateClient(Guid.NewGuid(), "Elizabeth", new List<PhoneDto> { primaryPhone }, null, DateTime.Now);
            var table = PluginContext.Operations.GetTables().First();

            editSession.CreateReserve(DateTime.Now + TimeSpan.FromMinutes(30), client, table);
            SubmitChanges(editSession);
        }

        private void btnBindReserve_Click(object sender, RoutedEventArgs e)
        {
            var editSession = PluginContext.Operations.CreateEditSession();

            var client = editSession.CreateClient(Guid.NewGuid(), "Elizabeth", new List<PhoneDto> { primaryPhone }, null, DateTime.Now);
            var table = PluginContext.Operations.GetTables().First();
            var order = editSession.CreateOrder(table);
            var guest = editSession.AddOrderGuest("Elizabeth", order);
            var orderItemProduct = PluginContext.Operations.GetHierarchicalMenu().Products.Last();
            editSession.AddOrderProductItem(1m, orderItemProduct, order, guest, null);
            editSession.ChangeEstimatedOrderGuestsCount(2, order);

            var reserve = editSession.CreateReserve(DateTime.Now + TimeSpan.FromMinutes(30), client, table);
            editSession.BindReserveToOrder(reserve, order);

            SubmitChanges(editSession);
        }

        private void btnCancelReserve_Click(object sender, RoutedEventArgs e)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var createdReserve = PluginContext.Operations.GetReserves().LastOrDefault(r => r.Status == ReserveStatus.New);

            if (createdReserve != null)
                try
                {
                    PluginContext.Operations.CancelReserve(credentials, createdReserve, ReserveCancelReason.ClientRefused);
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", "Cancel Reserve", Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
                MessageBox.Show("Reserve wasn't found!");
        }

        private void btnPrintReserve_Click(object sender, RoutedEventArgs e)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var createdReserve = PluginContext.Operations.GetReserves().LastOrDefault(r => r.Status == ReserveStatus.New);

            if (createdReserve != null)
                PluginContext.Operations.PrintTableReservedCheque(credentials, createdReserve);
            else
                MessageBox.Show("Reserve wasn't found!");
        }

        private void btnChangeGuestCount_Click(object sender, RoutedEventArgs e)
        {
            var createdReserve = PluginContext.Operations.GetReserves().LastOrDefault();

            if (createdReserve != null)
            {
                var editSession = PluginContext.Operations.CreateEditSession();

                if (createdReserve.Order != null)
                    editSession.ChangeEstimatedOrderGuestsCount(7, createdReserve.Order);
                editSession.ChangeReserveGuestsCount(7, createdReserve);
                try
                {
                    SubmitChanges(editSession);
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", "Change Guest Count", Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Reserve wasn't found!");
        }

        private void btnChangeComment_Click(object sender, RoutedEventArgs e)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var createdReserve = PluginContext.Operations.GetReserves().LastOrDefault();

            if (createdReserve != null)
                try
                {
                    PluginContext.Operations.ChangeReserveComment(createdReserve.Comment + " New comment.", createdReserve, credentials);
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", "Change Comment", Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
                MessageBox.Show("Reserve wasn't found!");
        }

        private void btnChangeTable_Click(object sender, RoutedEventArgs e)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var createdReserve = PluginContext.Operations.GetReserves().LastOrDefault();
            var newTable = PluginContext.Operations.GetTables().Last();

            if (createdReserve != null)
                try
                {
                    PluginContext.Operations.ChangeReserveTable(newTable, createdReserve, credentials);
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", "Change Table Number", Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
                MessageBox.Show("Reserve wasn't found!");
        }
    }
}
