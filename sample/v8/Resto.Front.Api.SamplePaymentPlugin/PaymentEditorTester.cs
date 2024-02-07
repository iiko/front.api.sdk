using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Device.Tasks;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePaymentPlugin
{
    internal sealed class PaymentEditorTester : IDisposable
    {
        private Window window;
        private ListBox buttons;

        public PaymentEditorTester()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void EntryPoint()
        {
            buttons = new ListBox();
            window = new Window
            {
                Content = buttons,
                Width = 310,
                Height = 870,
                Topmost = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                ResizeMode = ResizeMode.CanResize,
                ShowInTaskbar = true,
                VerticalContentAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.SingleBorderWindow,
            };

            AddButton("Add cash payment", AddCashPayment);
            AddButton("Add card payment", AddCardPayment);
            AddButton("Add writeoff payment", AddWriteoffPayment);

            AddSeparator();

            AddButton("Add cash preliminary payment", AddCashPreliminaryPayment);
            AddButton("Add card preliminary payment", AddCardPreliminaryPayment);
            AddButton("Add credit preliminary payment", AddCreditPreliminaryPayment);
            AddButton("Delete preliminary payment", DeletePreliminaryPayment);
            AddButton("Add cash preliminary payment with discount", AddCashPreliminaryPaymentWithDiscount);
            AddButton("Delete preliminary payment with discount", DeletePreliminaryPaymentWithDiscount);

            AddSeparator();

            AddButton("Add cash external not processed payment", AddCashExternalNotProcessedPayment);
            AddButton("Add card external not processed payment", AddCardExternalNotProcessedPayment);
            AddButton("Add writeoff external not processed payment", AddWriteoffExternalNotProcessedPayment);
            AddButton("Add plugin external not processed payment", AddPluginExternalNotProcessedPayment);
            AddButton("Add cash external processed payment", AddCashExternalProcessedPayment);
            AddButton("Add card external processed payment", AddCardExternalProcessedPayment);

            AddSeparator();

            AddButton("Add cash external processed prepay", AddCashExternalProcessedPrepay);
            AddButton("Add card external processed prepay", AddCardExternalProcessedPrepay);
            AddButton("Add plugin external not processed prepay", AddPluginExternalNotProcessedPrepay);

            AddSeparator();

            AddButton("Unprocess cash external payment", UnprocessCashExternalPrepay);
            AddButton("Unprocess card external payment", UnprocessCardExternalPrepay);
            AddButton("Unprocess plugin external payment", UnprocessPluginExternalPrepay);
            AddSeparator();

            AddButton("Add cash processed donation", AddCashProcessedDonation);
            AddButton("Add card processed donation", AddCardProcessedDonation);
            AddButton("Add plugin processed donation", AddPluginProcessedDonation);
            AddButton("Add plugin not processed donation", AddPluginNotProcessedDonation);
            AddButton("Delete donation", DeleteDonation);

            AddSeparator();

            AddButton("Add external fiscalized payment", AddExternalFiscalizedPayment);
            AddButton("Add external fiscalized prepay", AddExternalFiscalizedPrepay);
            AddButton("Delete external fiscalized payment", DeleteExternalFiscalizedPayment);

            AddSeparator();

            AddButton("Pay order on cash local", PayOrderOnCashLocalAndPayOutOnUser);
            AddButton("Pay order on cash remote", PayOrderOnCashRemoteAndPayOutOnUser);
            AddButton("Pay order on card local", PayOrderOnCardLocalAndPayOutOnUser);
            AddButton("Pay order on card remote", PayOrderOnCardRemoteAndPayOutOnUser);
            AddButton("Pay order with existing payments local", PayOrderWithExistingPaymentsLocal);
            AddButton("Pay order with existing payments remote", PayOrderWithExistingPaymentsRemote);
            AddButton("Storno order", StornoOrder);

            AddSeparator();

            AddButton("Storno past order", StornoPastOrder);
            AddButton("Storno products", StornoProducts);

            AddSeparator();

            AddButton("Open cafe session", OpenCafeSession);
            AddButton("Close cafe session", CloseCafeSession);

            AddSeparator();

            AddButton("Create order and process prepayments", CreateOrderAndProcessPrepayments);
            AddButton("Add and process preliminary payment", AddAndProcessPreliminaryPayment);

            window.ShowDialog();
        }

        private void AddSeparator()
        {
            buttons.Items.Add(new Separator { Margin = new Thickness(0, 4, 0, 4) });
        }

        private void AddButton(string text, Action action)
        {
            var button = new Button { Content = text, Margin = new Thickness(2) };
            button.Click += (s, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    var message = $"{ex.GetType()}{Environment.NewLine}" +
                        $"Cannot {text} :-({Environment.NewLine}" +
                        $"Message: {ex.Message}{Environment.NewLine}" +
                        $"{(ex is PaymentActionFailedException fex ? $"Reason: {fex.Reason}{Environment.NewLine}Details: {fex.Details}{Environment.NewLine}" : string.Empty)}" +
                        $"{ex.StackTrace}";
                    MessageBox.Show(message, GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            buttons.Items.Add(button);
        }

        #region Add payment

        /// <summary>
        /// Добавление обычного платежа наличными.
        /// </summary>
        private void AddCashPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            PluginContext.Operations.AddPaymentItem(50, null, paymentType, order, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Добавление обычного платежа картой.
        /// </summary>
        private void AddCardPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            PluginContext.Operations.AddPaymentItem(50, additionalData, paymentType, order, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Добавление обычного платежа без выручки.
        /// </summary>
        private void AddWriteoffPayment()
        {
            // Сотрудник, у которого будет проверяться право F_COTH (Закрывать заказы за счет заведения).
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Writeoff);
            var additionalData = new WriteoffPaymentItemAdditionalData
            {
                Ratio = 1,
                Reason = "Списание",
                // Сотрудник или гость, на которого производится списание.
                AuthorizationUser = PluginContext.Operations.GetUsers().SingleOrDefault(user => user.Name == "Гость Григорий")
            };
            PluginContext.Operations.AddPaymentItem(order.ResultSum, additionalData, paymentType, order, credentials);
        }

        #endregion Add payment

        #region Add or delete preliminary payment

        /// <summary>
        /// Добавление отложенного платежа наличными.
        /// </summary>
        private void AddCashPreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            PluginContext.Operations.AddPreliminaryPaymentItem(100, null, paymentType, deliveryOrder, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Добавление отложенного платежа картой.
        /// </summary>
        private void AddCardPreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            PluginContext.Operations.AddPreliminaryPaymentItem(150, additionalData, paymentType, deliveryOrder, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Добавление отложенного платежа в кредит на контрагента.
        /// </summary>
        private void AddCreditPreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Credit && x.Name.ToUpper().StartsWith("БЕЗН"));
            var user = PluginContext.Operations.GetUsers().Last(u => u.Name.ToUpper().StartsWith("BBB"));
            var additionalData = new CreditPaymentItemAdditionalData { CounteragentUserId = user.Id };
            PluginContext.Operations.AddPreliminaryPaymentItem(200, additionalData, paymentType, deliveryOrder, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление отложенного платежа.
        /// </summary>
        private void DeletePreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentItem = deliveryOrder.Payments.First(i => i.IsPreliminary);
            PluginContext.Operations.DeletePreliminaryPaymentItem(paymentItem, deliveryOrder, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Добавление отложенного платежа наличными со скидкой из типа оплаты.
        /// </summary>
        private void AddCashPreliminaryPaymentWithDiscount()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash && x.DiscountType != null);
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.AddDiscount(paymentType.DiscountType, deliveryOrder);
            editSession.AddPreliminaryPaymentItem(100, null, paymentType, deliveryOrder);
            PluginContext.Operations.SubmitChanges(editSession, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление отложенного платежа со скидкой из типа оплаты.
        /// </summary>
        private void DeletePreliminaryPaymentWithDiscount()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentItem = deliveryOrder.Payments.First(i => i.IsPreliminary && i.Type.DiscountType != null);
            var discountItem = deliveryOrder.Discounts.Last(d => d.DiscountType.Equals(paymentItem.Type.DiscountType));
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.DeleteDiscount(discountItem, deliveryOrder);
            editSession.DeletePreliminaryPaymentItem(paymentItem, deliveryOrder);
            PluginContext.Operations.SubmitChanges(editSession, PluginContext.Operations.GetDefaultCredentials());
        }

        #endregion Add or delete preliminary payment

        #region Add or delete external payment

        /// <summary>
        /// Добавление внешнего непроведенного платежа наличными.
        /// </summary>
        private void AddCashExternalNotProcessedPayment()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, null, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего непроведенного платежа картой.
        /// </summary>
        private void AddCardExternalNotProcessedPayment()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, additionalData, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего непроведенного платежа без выручки.
        /// </summary>
        private void AddWriteoffExternalNotProcessedPayment()
        {
            const bool isProcessed = false;
            // Сотрудник, у которого будет проверяться право F_COTH (Закрывать заказы за счет заведения).
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Writeoff);
            var additionalData = new WriteoffPaymentItemAdditionalData
            {
                Ratio = 1,
                Reason = "Списание",
                // Сотрудник или гость, на которого производится списание.
                AuthorizationUser = PluginContext.Operations.GetUsers().SingleOrDefault(user => user.Name == "Гость Григорий")
            };
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum, isProcessed, additionalData, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего плагинного непроведенного платежа.
        /// </summary>
        private void AddPluginExternalNotProcessedPayment()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var additionalData = new ExternalPaymentItemAdditionalData { CustomData = Serializer.Serialize(new PaymentAdditionalData { SilentPay = true }) };
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, additionalData, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего проведенного платежа наличными.
        /// </summary>
        private void AddCashExternalProcessedPayment()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, null, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего проведенного платежа картой.
        /// </summary>
        private void AddCardExternalProcessedPayment()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, additionalData, null, paymentType, order, credentials);
        }

        #endregion Add or delete external payment

        #region Add and process prepay

        /// <summary>
        /// Добавление внешнего проведенного платежа наличными с превращением его в предоплату в iiko.
        /// </summary>
        private void AddCashExternalProcessedPrepay()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var paymentItem = PluginContext.Operations.AddExternalPaymentItem(150, isProcessed, null, null, paymentType, order, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);

            PluginContext.Operations.ProcessPrepay(order, paymentItem, credentials);
        }

        /// <summary>
        /// Добавление внешнего проведенного платежа картой с превращением его в предоплату в iiko.
        /// </summary>
        private void AddCardExternalProcessedPrepay()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var paymentItem = PluginContext.Operations.AddExternalPaymentItem(150, isProcessed, additionalData, null, paymentType, order, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);

            PluginContext.Operations.ProcessPrepay(order, paymentItem, credentials);
        }

        /// <summary>
        /// Добавление внешнего плагинного непроведенного платежа с превращением его в предоплату в iiko.
        /// </summary>
        private void AddPluginExternalNotProcessedPrepay()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var additionalData = new ExternalPaymentItemAdditionalData { CustomData = Serializer.Serialize(new PaymentAdditionalData { SilentPay = true }) };
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var paymentItem = PluginContext.Operations.AddExternalPaymentItem(150, isProcessed, additionalData, null, paymentType, order, credentials);

            PluginContext.Operations.ProcessPrepay(order, paymentItem, credentials);
        }

        #endregion Add and process prepay

        #region Add and process or delete donation

        private void AddCashProcessedDonation()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.Cash));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetDefaultCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(order, donationType, paymentType, null, isProcessed, order.ResultSum / 10, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void AddCardProcessedDonation()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.Closed);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.Card));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetDefaultCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(order, donationType, paymentType, additionalData, isProcessed, order.ResultSum / 4, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void AddPluginProcessedDonation()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.External));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var credentials = PluginContext.Operations.GetDefaultCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(order, donationType, paymentType, null, isProcessed, order.ResultSum / 3, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void AddPluginNotProcessedDonation()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.External));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var additionalData = new ExternalPaymentItemAdditionalData { CustomData = Serializer.Serialize(new PaymentAdditionalData { SilentPay = true }) };
            var credentials = PluginContext.Operations.GetDefaultCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(order, donationType, paymentType, additionalData, isProcessed, order.ResultSum / 2, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void DeleteDonation()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill || o.Status == OrderStatus.Closed);
            var paymentItem = order.Donations.Last();
            PluginContext.Operations.DeleteDonation(order, paymentItem, PluginContext.Operations.GetDefaultCredentials());
        }

        #endregion Add and process or delete donation

        #region Add and process or delete external fiscalized payment

        /// <summary>
        /// Добавление внешнего фискализованного платежа картой.
        /// </summary>
        private void AddExternalFiscalizedPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetDefaultCredentials();

            PluginContext.Operations.AddExternalFiscalizedPaymentItem(50, additionalData, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего фискализованного платежа картой с превращением его в предоплату в iiko.
        /// </summary>
        private void AddExternalFiscalizedPrepay()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var paymentItem = PluginContext.Operations.AddExternalFiscalizedPaymentItem(50, additionalData, paymentType, order, credentials);

            order = PluginContext.Operations.GetOrderById(order.Id);
            PluginContext.Operations.ProcessPrepay(order, paymentItem, credentials);
        }

        /// <summary>
        /// Удаление внешнего фискализованного платежа.
        /// </summary>
        private void DeleteExternalFiscalizedPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentItem = order.Payments.Last(i => i.IsFiscalizedExternally);

            PluginContext.Operations.DeleteExternalFiscalizedPaymentItem(paymentItem, order, PluginContext.Operations.GetDefaultCredentials());
        }

        #endregion Add and process or delete external fiscalized payment

        #region Pay order

        /// <summary>
        /// Дистанционная (до-)оплата заказа наличными локально. Наличные учитываются, как задолженность сотрудника.
        /// </summary>
        private void PayOrderOnCashLocalAndPayOutOnUser()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypesToPayOutOnUser().First(x => x.Kind == PaymentTypeKind.Cash);
            // Payment is possible by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrderAndPayOutOnUser(order, true, paymentType, order.ResultSum, credentials);
        }

        /// <summary>
        /// Дистанционная (до-)оплата заказа наличными на главном терминале. Наличные учитываются, как задолженность сотрудника.
        /// </summary>
        private void PayOrderOnCashRemoteAndPayOutOnUser()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypesToPayOutOnUser().First(x => x.Kind == PaymentTypeKind.Cash);
            // Payment is possible by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrderAndPayOutOnUser(order, false, paymentType, order.ResultSum, credentials);
        }

        /// <summary>
        /// Дистанционная (до-)оплата заказа картой локально.
        /// </summary>
        private void PayOrderOnCardLocalAndPayOutOnUser()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypesToPayOutOnUser().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            // Payment is possible by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrderAndPayOutOnUser(order, true, paymentType, order.ResultSum, credentials);
        }

        /// <summary>
        /// Дистанционная (до-)оплата заказа картой на главном терминале.
        /// </summary>
        private void PayOrderOnCardRemoteAndPayOutOnUser()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypesToPayOutOnUser().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            // Payment is possible by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrderAndPayOutOnUser(order, false, paymentType, order.ResultSum, credentials);
        }

        /// <summary>
        /// Дистанционная оплата заказа существующими в заказе платежами локально.
        /// </summary>
        private void PayOrderWithExistingPaymentsLocal()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            // Payment is possible only by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrder(order, true, credentials);
        }

        /// <summary>
        /// Дистанционная оплата заказа существующими в заказе платежами на главном терминале.
        /// </summary>
        private void PayOrderWithExistingPaymentsRemote()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            // Payment is possible only by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrder(order, false, credentials);
        }

        /// <summary>
        /// Возврат чека заказа (сторнирование).
        /// </summary>
        private void StornoOrder()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.Closed);
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.StornoOrder(order, credentials);
        }

        #endregion Pay order

        private void UnprocessCardExternalPrepay()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var paymentItem = order.Payments.First(i => i.IsPrepay && i.Type.Kind == PaymentTypeKind.Card);

            PluginContext.Operations.UnprocessPayment(order, paymentItem, credentials);
        }

        private void UnprocessCashExternalPrepay()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var paymentItem = order.Payments.First(i => i.IsPrepay && i.Type.Kind == PaymentTypeKind.Cash);

            PluginContext.Operations.UnprocessPayment(order, paymentItem, credentials);
        }

        private void UnprocessPluginExternalPrepay()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var paymentItem = order.Payments.First(i => i.IsPrepay && i.Type.Kind == PaymentTypeKind.External);

            PluginContext.Operations.UnprocessPayment(order, paymentItem, credentials);
        }

        #region Storno past order items

        private void StornoPastOrder()
        {
            var pastOrder = PluginContext.Operations.GetPastOrders().First();
            var paymentType = PluginContext.Operations.GetPaymentTypesToStornoPastOrderItems().First();

            // rt.WriteoffType.HasFlag(WriteoffType.Cafe) - сторнирование со списанием
            // rt.WriteoffType == WriteoffType.None - сторнирование без списания
            var removalType = PluginContext.Operations.GetRemovalTypesToStornoPastOrderItems().First(rt => rt.WriteoffType.HasFlag(WriteoffType.Cafe));
            var section = PluginContext.Operations.GetTerminalsGroupRestaurantSections(PluginContext.Operations.GetHostTerminalsGroup()).First();
            var orderType = PluginContext.Operations.GetOrderTypes().FirstOrDefault(ot => ot.OrderServiceType == OrderServiceTypes.Common);
            var taxationSystem = PluginContext.Operations.GetTaxationSystemsToStornoPastOrderItems().Select(ts => (TaxationSystem?)ts).FirstOrDefault();

            PluginContext.Operations.StornoPastOrder(
                pastOrder,
                paymentType,
                removalType,
                section,
                orderType,
                PluginContext.Operations.GetDefaultCredentials(),
                null,
                taxationSystem,
                null);
        }

        private void StornoProducts()
        {
            var paymentType = PluginContext.Operations.GetPaymentTypesToStornoPastOrderItems().First(pt => pt.Name.Contains("SampleApiPayment"));

            // rt.WriteoffType.HasFlag(WriteoffType.Cafe) - сторнирование со списанием
            // rt.WriteoffType == WriteoffType.None - сторнирование без списания
            var removalType = PluginContext.Operations.GetRemovalTypesToStornoPastOrderItems().First(rt => rt.WriteoffType.HasFlag(WriteoffType.Cafe));
            var section = PluginContext.Operations.GetTerminalsGroupRestaurantSections(PluginContext.Operations.GetHostTerminalsGroup()).First();
            var orderType = PluginContext.Operations.GetOrderTypes().FirstOrDefault(ot => ot.OrderServiceType == OrderServiceTypes.Common);
            var taxationSystem = PluginContext.Operations.GetTaxationSystemsToStornoPastOrderItems().Select(ts => (TaxationSystem?)ts).FirstOrDefault();

            var activeProducts = PluginContext.Operations.GetActiveProducts()
                .Where(product => product.Template == null &&
                    (product.Type == ProductType.Dish || product.Type == ProductType.Modifier || product.Type == ProductType.Goods || product.Type == ProductType.ForPurchase))
                .ToList();

            PluginContext.Operations.StornoPastOrderItems(
                GetPastOrderItems(activeProducts).ToList(),
                paymentType,
                removalType,
                section,
                orderType,
                PluginContext.Operations.GetDefaultCredentials(),
                null,
                taxationSystem,
                null);
        }

        private IEnumerable<PastOrderItem> GetPastOrderItems(IReadOnlyList<IProduct> activeProducts)
        {
            var product = activeProducts.First(p => p.Name.Contains("Чай"));
            yield return new PastOrderItem
            {
                IsMainDish = true,
                Product = product,
                ProductSize = PluginContext.Operations.GetProductSizes().FirstOrDefault(ps => ps.Name.Contains("S")),
                Amount = 2,
                SumWithDiscounts = 100.5m,
                Price = 0, // не используется в StornoPastOrderItems
                SumWithoutDiscounts = 0, // не используется в StornoPastOrderItems
            };

            product = activeProducts.First(p => p.Name.Contains("Сливки"));
            yield return new PastOrderItem
            {
                IsMainDish = false,
                Product = product,
                ProductSize = null,
                Amount = 2,
                SumWithDiscounts = 0m,
                Price = 0, // не используется в StornoPastOrderItems
                SumWithoutDiscounts = 0, // не используется в StornoPastOrderItems
            };

            product = activeProducts.First(p => p.Name.Contains("Пюре"));
            yield return new PastOrderItem
            {
                IsMainDish = true,
                Product = product,
                ProductSize = null,
                Amount = 1,
                SumWithDiscounts = 90m,
                Price = 0, // не используется в StornoPastOrderItems
                SumWithoutDiscounts = 0, // не используется в StornoPastOrderItems
            };

            product = activeProducts.First(p => p.Name.Contains("Бутылка"));
            yield return new PastOrderItem
            {
                IsMainDish = true,
                Product = product,
                ProductSize = null,
                Amount = 2,
                SumWithDiscounts = -15m,
                Price = 0, // не используется в StornoPastOrderItems
                SumWithoutDiscounts = 0, // не используется в StornoPastOrderItems
            };
        }
        #endregion Storno past order items

        private void AddAndProcessPreliminaryPayment()
        {
            // код имитирует сценарий DM
            var order = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(i => i.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            // в существующую доставку добавим предваритальный платеж 1000р, с этой суммы гостю нужна сдача
            var paymentItem = PluginContext.Operations.AddPreliminaryPaymentItem(1000m, null, paymentType, order, credentials);

            // курьер доставил заказ, пора оплачивать
            PluginContext.Operations.ExecuteContinuousOperation(
                operations =>
                {
                    // изменим сумму предварительного платежа, здесь можно рассчитать сдачу (Сдача = СуммаПредвПлатежа - СуммаЗаказа)
                    operations.ChangePaymentItemSum(order.ResultSum, null, null, paymentItem, PluginContext.Operations.GetDeliveryOrderById(order.Id), credentials);
                    // предварительный платеж превращаем в предоплату
                    operations.ProcessPrepay(PluginContext.Operations.GetDeliveryOrderById(order.Id), paymentItem, credentials);
                });
        }

        private void CreateOrderAndProcessPrepayments()
        {
            PluginContext.Operations.ExecuteContinuousOperation( // запускаем длительную непрерывную операцию, используя всегда доступный глобальный PluginContext.Operations
                operations => // внутри этой операции используем полученный на вход существующий только на время этого вызова экземпляр operations
                {
                    var credentials = operations.GetDefaultCredentials();

                    // глобальный PluginContext.Operations здесь тоже допустимо использовать, если с ним уже написаны какие-то кеши, вспомогательные методы
                    // для выполнения операций чтения, не обязательно весь код переводить на локальный operations, в этой части оба экземпляра сервиса работают идентично:
                    var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.CanBeExternalProcessed);

                    // Разница в том, что, например, созданный через PluginContext.Operations заказ немедленно станет доступен для редактирования всем желающим
                    // - пользователь сможет вносить изменения в этот заказ;
                    // - другие плагины, если они следят за появлением заказов, смогут что-то в нём поменять;
                    // - фоновые встроенные обработчики смогут выполнить автопечать на кухню, автозавершение приготовления и прочие операции
                    // Иными словами, если мы собирались создать заказ и затем сделать с ним ещё что-то, нет гарантии, что мы сможем выполнить это подряд,
                    // заказ могут успеть «угнать» (мы получим исключение EntityAlreadyInUseException) и
                    // внести несовместимые с нашими планами изменения (мы получим EntityModifiedException).

                    // По возможности выполняем действия в рамках одной сессии редактирования — это будет атомарно, «всё или ничего».
                    // Однако, некоторые действия невозможно выполнить в рамках сессии редактирования (как правило, это необратимые действия),
                    // тогда цепочку таких действий можно выполнить подряд в рамках одной непрерывной операции (continuous operation).
                    // При этом все объекты, изменённые нами через operations, для всех остальных будут неявно заблокированы до конца этой операции
                    var editSession = operations.CreateEditSession();
                    var orderStub = editSession.CreateOrder(null);
                    editSession.AddOrderGuest("Herbert", orderStub);
                    var paymentItemStub = editSession.AddExternalPaymentItem(42m, true, null, null, paymentType, orderStub);
                    var submittedEntities = operations.SubmitChanges(editSession, credentials);

                    // Заказ уже создан, все его видят, но редактировать можем только мы                    
                    var order = submittedEntities.Get(orderStub);
                    var paymentItem = submittedEntities.Get(paymentItemStub);

                    // Важно не задерживаться в этом месте слишком долго, дабы позволить другим тоже вносить изменения
                    // Все необходимые данные желательно получить и подготовить заранее (особенно если для этого нужны сетевые запросы к внешним сервисам)
                    operations.ProcessPrepay(order, paymentItem, credentials);
                });
        }

        private void OpenCafeSession()
        {
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.OpenCafeSession(credentials);
        }

        private void CloseCafeSession()
        {
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.CloseCafeSession(credentials);
        }

        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
        }
    }

    [Serializable]
    public sealed class PaymentAdditionalData
    {
        public bool SilentPay { get; set; }
    }

    internal static class Serializer
    {
        internal static string Serialize<T>(T data) where T : class
        {
            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw))
            {
                new XmlSerializer(typeof(T)).Serialize(writer, data);
                return sw.ToString();
            }
        }
    }
}
