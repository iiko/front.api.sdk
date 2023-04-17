using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Extensions;
using System.Windows.Controls;
using System.Threading;
using System.Xml.Linq;
using Resto.Front.Api.SamplePlugin.WpfHelpers;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.JournalEvents;
using Resto.Front.Api.Data.Print;
using Resto.Front.Api.Editors.Stubs;
using Resto.Front.Api.UI;
using Button = System.Windows.Controls.Button;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class EditorTester : IDisposable
    {
        private Window window;
        private ItemsControl buttons;
        private const string PluginName = "Resto.Front.Api.SamplePlugin";
        private readonly Random rnd = new Random();
        private CheckBox chkUseCurrentOrder;

        public EditorTester()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void EntryPoint()
        {
            buttons = new ItemsControl();
            window = new Window
            {
                Title = "Sample plugin",
                Content = new ScrollViewer
                {
                    Content = buttons
                },
                Width = 200,
                Height = 500,
                Topmost = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                ResizeMode = ResizeMode.CanMinimize,
                ShowInTaskbar = true,
                VerticalContentAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.SingleBorderWindow,
                SizeToContent = SizeToContent.Width
            };

            AddOrderSelectionCheckBox();
            AddButton("Print X-Report", PrintXReport);
            AddButton("Create order", CreateOrder);
            AddButton("Create order to another waiter", CreateOrderToWaiter);
            AddButton("Add guest", AddGuest);
            AddButton("Add product", AddProduct);
            AddButton("Add modifier", AddModifier);
            AddButton("Add group modifier", AddGroupModifier);
            AddButton("Increase amount", IncreaseAmount);
            AddButton("Decrease amount", DecreaseAmount);
            AddButton("Set amount", SetAmount);
            AddButton("Move product", MoveProductToNewGuest);
            AddButton("Split product", SplitProduct);
            AddButton("Move to another waiter", MoveToAnotherWaiter);
            AddButton("Print product", PrintProduct);
            AddButton("Print order", PrintOrder);
            AddButton("Print bill cheque", PrintBillCheque);
            AddButton("Bill order", BillOrder);
            AddButton("Cancel Bill", CancelBill);
            AddButton("Delete guest", DeleteGuest);
            AddButton("Delete product", DeleteProduct);
            AddButton("Delete printed product", DeletePrintedProduct);
            AddButton("Delete modifier", DeleteModifier);
            AddButton("Delete printed modifier", DeletePrintedModifier);
            AddButton("Add product comment", AddProductComment);
            AddButton("Delete product comment", DeleteProductComment);
            AddButton("Create delivery", CreateDelivery);
            AddButton("Create delivery with source key", CreateDeliveryWithOriginName);
            AddButton("Change self-service delivery on courier", ChangeDeliveryOrderTypeOnCourier);
            AddButton("Change delivery by courier on self-service", ChangeDeliveryOrderTypeOnSelfService);
            AddButton("Change delivery comment", ChangeDeliveryComment);
            AddButton("Show deliveries with not empty source key", ShowDeliveriesWithNotEmptyOriginName);
            AddButton("Create and send delivery", CreateAndSendDelivery);
            AddButton("Set delivery delivered", SetDeliveryDelivered);
            AddButton("Set delivery undelivered", SetDeliveryUndelivered);
            AddButton("Set delivery closed", SetDeliveryClosed);
            AddButton("Set delivery unclosed", SetDeliveryUnclosed);
            AddButton("Split order", SplitOrder);
            AddButton("Add Discount", AddDiscount);
            AddButton("Add Flexible Sum Discount", AddFlexibleSumDiscount);
            AddButton("Create discount card", CreateDiscountCard);
            AddButton("Update discount card", UpdateDiscountCard);
            AddButton("Add client to order", AddClientToOrder);
            AddButton("Remove order client", RemoveOrderClient);
            AddButton("Add discount by card number", AddDiscountByCardNumber);
            AddButton("Delete discount", DeleteDiscount);
            AddButton("AddSelectiveDiscount", AddSelectiveDiscount);
            AddButton("Add compound item", AddCompoundItem);
            AddButton("Add splitted compound item", AddSplittedCompoundItem);
            AddButton("Add combo in order", AddComboInOrder);
            AddButton("Add external data in order", AddExternalDataToOrder);
            AddButton("Delete external data from order", DeleteExternalDataFromOrder);
            AddButton("Cancel new delivery", CancelNewDelivery);
            AddButton("Delete order and hide items", DeleteOrderAndHideItemsFromOlap);
            AddButton("Cancel new delivery and and hide items", CancelNewDeliveryAndHideItemsFromOlap);
            AddButton("Print check", PrintCheck);
            AddButton("Add journal events", CreateJournalEvents);
            AddButton("Add product and change open price", AddProductAndSetOpenPrice);
            AddButton("Change price category", ChangePriceCategory);
            AddButton("Reset price category", ResetPriceCategory);
            AddButton("Start service", StartService);
            AddButton("Stop service", StopService);
            AddButton("Change order tables", ChangeOrderTables);
            AddButton("Mark order as tab", MarkOrderAsTab);
            AddButton("Show allergens", ShowAllergens);
            AddButton("Change order comment", ChangeOrderComment);
            AddButton("Change order public data", AddPublicExternalData);
            AddButton("Change cooking items cooking place", ChangeCookingItemsCookingPlace);
            AddButton("Navigate to current order", NavigateToCurrentOrder);
            AddButton("Navigate to other order", NavigateToOtherOrder);
            AddButton("Add or update kitchen order external data", AddOrUpdateKitchenOrderExternalData);
            AddButton("Delete external data from kitchen order", DeleteKitchenOrderExternalData);
            AddButton("Try get kitchen order external data", TryGetByIdKitchenOrderExternalData);
            AddButton("Get past order by id", GetPastOrderById);
            AddButton("Get past orders by sum", GetPastOrdersBySum);
            AddButton("Get past orders by number", GetPastOrdersByNumber);

            window.ShowDialog();
        }

        private void AddOrderSelectionCheckBox()
        {
            chkUseCurrentOrder = new CheckBox
            {
                Content = "Apply changes to order that is opened in iikoFront",
                Margin = new Thickness(4)
            };
            buttons.Items.Add(chkUseCurrentOrder);
        }

        private void AddButton(string text, Action action)
        {
            var button = new Button
            {
                Content = text,
                Margin = new Thickness(2),
                Padding = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };
            button.Click += (s, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", text, Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            buttons.Items.Add(button);
        }

        private void AddButton(string text, Action<IOrder, IOperationService> action)
        {
            AddButton(text, () =>
            {
                if (chkUseCurrentOrder.IsChecked == true)
                    PluginContext.Operations.TryEditCurrentOrder(x => action(x.order, x.os));
                else
                    action(PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New), PluginContext.Operations);
            });
        }

        private void AddButton(string text, Action<IOrder, IOperationService, IViewManager> action)
        {
            AddButton(text, () =>
            {
                if (chkUseCurrentOrder.IsChecked == true)
                    PluginContext.Operations.TryEditCurrentOrder(x => action(x.order, x.os, x.vm));
                else
                    throw new NotSupportedException($"This button's handler needs view manager to show dialog, which is not available in background mode. Set the “{chkUseCurrentOrder.Content}” checkbox to interact with user.");
            });

        }

        /// <summary>
        /// Получение и печать X-Отчёта.
        /// </summary>   
        private void PrintXReport()
        {
            //const string reportId = "SERVER_SALES_REPORT";
            //const string reportId = "CAFE_SESSION_FULL_REPORT";
            //const string reportId = "ORDERS_SUMMARY_REPORT";
            const string reportId = "X_REPORT";

            var cashRegister = PluginContext.Operations.GetHostTerminalPointsOfSale().FirstOrDefault()?.CashRegister;
            //ICashRegisterInfo cashRegister = null;

            try
            {
                var doc = PluginContext.Operations.GetReportMarkupById(reportId, cashRegister);

                var printer = PluginContext.Operations.GetReportPrinter();

                if (PluginContext.Operations.Print(printer, doc))
                    MessageBox.Show("Report successfully printed", "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.None);
                else
                    MessageBox.Show("Print error", "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// Создание заказа с добавлением гостя Alex и продукта номенклатуры.
        /// </summary>   
        private void CreateOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var product1 = GetProduct();
            var product2 = GetProduct();

            // создаём заказ, добавляем в него пару гостей, одному из гостей добавляем блюдо, и всё это атомарно в одной сессии
            var editSession = PluginContext.Operations.CreateEditSession();
            var newOrder = editSession.CreateOrder(null); // заказ будет создан на столе по умолчанию
            editSession.ChangeOrderOriginName("Sample Plugin", newOrder);
            var guest1 = editSession.AddOrderGuest(null, newOrder); // настоящего гостя ещё нет (он будет после SubmitChanges), ссылаемся на будущего гостя через INewOrderGuestItemStub
            var guest2 = editSession.AddOrderGuest("Alex", newOrder);
            editSession.AddOrderProductItem(2m, product1, newOrder, guest1, null);
            var result = PluginContext.Operations.SubmitChanges(editSession);

            var previouslyCreatedOrder = result.Get(newOrder);
            var previouslyAddedGuest = result.Get(guest2); // настоящие гости уже есть, можно ссылаться напрямую на нужного гостя через IOrderGuestItem

            PluginContext.Operations.AddOrderProductItem(17.3m, product2, previouslyCreatedOrder, previouslyAddedGuest, null, credentials);
        }

        /// <summary>
        /// Создание заказа на конкретного официанта с добавлением гостя Alex и продукта номенклатуры.
        /// </summary>   
        private void CreateOrderToWaiter()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var product = GetProduct();

            var editSession = PluginContext.Operations.CreateEditSession();
            if (!ChooseItemDialogHelper.ShowDialog(PluginContext.Operations.GetUsers(), user => user.Name, out var selectedUser, "Select waiter", window))
                return;
            var newOrder = editSession.CreateOrder(null, waiter: selectedUser); // заказ будет создан на столе по умолчанию
            editSession.ChangeOrderOriginName("Sample Plugin", newOrder);
            editSession.AddOrderGuest(null, newOrder);
            var guest2 = editSession.AddOrderGuest("Alex", newOrder);
            var result = PluginContext.Operations.SubmitChanges(editSession);

            var previouslyCreatedOrder = result.Get(newOrder);
            var previouslyAddedGuest = result.Get(guest2);

            PluginContext.Operations.AddOrderProductItem(17.3m, product, previouslyCreatedOrder, previouslyAddedGuest, null, credentials);
        }

        /// <summary>
        /// Добавление гостя John Doe. 
        /// </summary>
        private static void AddGuest([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            os.AddOrderGuest("John Doe", order, credentials);
        }

        /// <summary>
        /// Привязать гостя Semen с номером карты "0123456789" к заказу.
        /// </summary>
        private static void AddClientToOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            var discountType = os.GetDiscountTypes().LastOrDefault(d => d.CanApplyByDiscountCard);
            const string cardNumber = "0123456789";
            const string clientName = "Semen";

            var client = os.CreateClient(Guid.NewGuid(), clientName, null, cardNumber, DateTime.Now, credentials);

            var card = os.SearchDiscountCardByNumber(cardNumber);
            if (card == null)
                os.CreateDiscountCard(cardNumber, clientName, null, discountType);
            else
                os.UpdateDiscountCard(card.Id, clientName, null, discountType);

            os.AddClientToOrder(credentials, order, client);
        }

        /// <summary>
        /// Отвязать гостя Semen от заказа.
        /// </summary>
        private static void RemoveOrderClient([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            var client = os.SearchClients(credentials, "Semen").Last();
            os.RemoveOrderClient(credentials, order, client);
        }

        /// <summary>
        /// Добавление продукта из номенклатуры.
        /// </summary>
        private void AddProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            var product = GetProduct();
            var guest = order.Guests.Last();
            var addedItem = os.AddOrderProductItem(42m, product, order, guest, null, credentials);

            if (product.ImmediateCookingStart)
                os.PrintOrderItems(credentials, os.GetOrderById(order.Id), new[] { addedItem });
        }

        private IProduct GetProduct(bool isCompound = false)
        {
            var activeProducts = PluginContext.Operations.GetActiveProducts()
                .Where(product =>
                    isCompound
                        ? product.Template != null
                        : product.Template == null
                        && product.Type == ProductType.Dish)
                .ToList();

            var index = rnd.Next(activeProducts.Count);
            return activeProducts[index];
        }

        /// <summary>
        /// Добавление составного блюда.
        /// </summary>
        private void AddCompoundItem([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var guest = order.Guests.Last();
            var product = GetProduct(true);
            var template = product.Template;
            // ReSharper disable once PossibleNullReferenceException
            var size = template.Scale != null ? os.GetProductScaleSizes(template.Scale).First() : null;
            var editSession = os.CreateEditSession();
            var compoundItem = editSession.AddOrderCompoundItem(product, order, guest, size);
            var primaryComponent = editSession.AddPrimaryComponent(product, order, compoundItem);
            editSession.ChangeOrderCookingItemAmount(42m, compoundItem, order);
            var modifierDefaultAmounts = os.GetTemplatedModifiersParamsByProduct(product);
            AddDefaultCompoundItemModifiers(compoundItem, template, modifierDefaultAmounts, order, editSession);
            AddDefaultCompoundItemComponentModifiers(primaryComponent, template, modifierDefaultAmounts, order, editSession);

            os.SubmitChanges(editSession);
        }

        /// <summary>
        /// Добавление составного блюда.
        /// </summary>
        private static void AddSplittedCompoundItem([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var editSession = os.CreateEditSession();
            AddSplittedCompoundItemInternal(order, editSession);

            os.SubmitChanges(editSession);
        }

        private static INewOrderCompoundItemStub AddSplittedCompoundItemInternal([NotNull] IOrder order, [NotNull] IEditSession editSession)
        {
            var guest = order.Guests.Last();
            var templateProducts = PluginContext.Operations.GetActiveProducts()
                .GroupBy(product => product.Template)
                .Last(group => group.Key != null && group.Count() >= 2);
            var template = templateProducts.Key;
            var size = template.Scale != null ? PluginContext.Operations.GetProductScaleSizes(template.Scale).First() : null;
            var compoundItem = editSession.AddOrderCompoundItem(templateProducts.First(), order, guest, size);
            editSession.AddPrimaryComponent(templateProducts.First(), order, compoundItem);
            editSession.AddSecondaryComponent(templateProducts.Skip(1).First(), order, compoundItem);
            return compoundItem;
        }

        private static void AddDefaultCompoundItemModifiers(IOrderCompoundItemStub compoundItem, ICompoundItemTemplate template, IReadOnlyList<ITemplatedModifierParams> modifierParams, IOrder order, IEditSession editSession)
        {
            foreach (var commonGroupModifier in template.GetCommonGroupModifiers(order.PriceCategory))
            {
                foreach (var commonChildModifier in commonGroupModifier.Items)
                {
                    var defaultAmount = GetDefaultModifierAmount(modifierParams, commonGroupModifier.ProductGroup, commonChildModifier.Product, commonChildModifier.DefaultAmount);
                    if (defaultAmount > 0)
                        editSession.AddOrderModifierItem(defaultAmount, commonChildModifier.Product, commonGroupModifier.ProductGroup, order, compoundItem);
                }
            }

            foreach (var commonSimpleModifier in template.GetCommonSimpleModifiers(order.PriceCategory))
            {
                var defaultAmount = GetDefaultModifierAmount(modifierParams, null, commonSimpleModifier.Product, commonSimpleModifier.DefaultAmount);
                if (defaultAmount > 0)
                    editSession.AddOrderModifierItem(defaultAmount, commonSimpleModifier.Product, null, order, compoundItem);
            }
        }

        private static void AddDefaultCompoundItemComponentModifiers(IOrderCompoundItemComponentStub component, ICompoundItemTemplate template, IReadOnlyList<ITemplatedModifierParams> modifierParams, IOrder order, IEditSession editSession)
        {
            foreach (var splittableGroupModifier in template.GetSplittableGroupModifiers(order.PriceCategory))
            {
                foreach (var splittableChildModifier in splittableGroupModifier.Items)
                {
                    var defaultAmount = GetDefaultModifierAmount(modifierParams, splittableGroupModifier.ProductGroup, splittableChildModifier.Product, splittableChildModifier.DefaultAmount);
                    if (defaultAmount > 0)
                        editSession.AddOrderModifierItem(defaultAmount, splittableChildModifier.Product, splittableGroupModifier.ProductGroup, order, component);
                }
            }

            foreach (var splittableSimpleModifier in template.GetSplittableSimpleModifiers(order.PriceCategory))
            {
                var defaultAmount = GetDefaultModifierAmount(modifierParams, null, splittableSimpleModifier.Product, splittableSimpleModifier.DefaultAmount);
                if (defaultAmount > 0)
                    editSession.AddOrderModifierItem(defaultAmount, splittableSimpleModifier.Product, null, order, component);
            }
        }

        private static int GetDefaultModifierAmount(IReadOnlyList<ITemplatedModifierParams> modifierParams, IProductGroup modifierGroup, IProduct modifier, int generalDefaultAmount)
        {
            var specificAmount = modifierParams.SingleOrDefault(x => Equals(x.ProductGroup, modifierGroup) && Equals(x.Product, modifier));
            return specificAmount?.DefaultAmount ?? generalDefaultAmount;
        }

        /// <summary>
        /// Добавление одиночного модификатора.
        /// </summary>
        private static void AddModifier([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var modifier = orderItem.AvailableSimpleModifiers.First();
            os.AddOrderModifierItem(1, modifier.Product, null, order, orderItem, os.GetCredentials());
        }

        /// <summary>
        /// Добавление группового модификатора.
        /// </summary>
        private static void AddGroupModifier([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var groupModifier = orderItem.AvailableGroupModifiers.Last();
            os.AddOrderModifierItem(1, groupModifier.Items.Last().Product, groupModifier.ProductGroup, order, orderItem, os.GetCredentials());
        }

        /// <summary>
        /// Увеличение количества последнего блюда заказа на 1.
        /// </summary>
        private static void IncreaseAmount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            os.ChangeOrderCookingItemAmount(orderItem.Amount + 1, orderItem, order, os.GetCredentials());
        }

        /// <summary>
        /// Уменьшение количества последнего блюда заказа на 1.
        /// </summary>
        private static void DecreaseAmount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            os.ChangeOrderCookingItemAmount(orderItem.Amount - 1, orderItem, order, os.GetCredentials());
        }

        /// <summary>
        /// Задание количества последнего блюда заказа.
        /// </summary>
        private static void SetAmount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            var amount = 1.1m;
            os.ChangeOrderCookingItemAmount(amount, orderItem, order, os.GetCredentials());
        }

        /// <summary>
        /// Создание нового гостя и перенос ему последнего блюда заказа.
        /// </summary>
        private static void MoveProductToNewGuest([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.Last();
            var editSession = os.CreateEditSession();
            var guest = editSession.AddOrderGuest("New guest", order);
            editSession.MoveOrderItemToAnotherGuest(orderItem, guest, order);

            os.SubmitChanges(editSession);
        }

        private static void MoveToAnotherWaiter([NotNull] IOrder order, [NotNull] IOperationService os, [NotNull] IViewManager vm)
        {
            var users = PluginContext.Operations.GetUsers();
            var currentUserIndex = users.ToList().IndexOf(order.Waiter);
            var selectedUserIndex = vm.ShowChooserPopup("Select new waiter", users.Select(u => u.Name).ToList(), currentUserIndex);
            if (selectedUserIndex < 0)
                return;

            var selectedUser = users[selectedUserIndex];
            os.ChangeOrderWaiter(order, selectedUser, os.GetCredentials());
        }

        /// <summary>
        /// Разделение последнего блюда на две части и назначение получившегося блюда на другого гостя (если есть)
        /// </summary>
        private static void SplitProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            var amountToSplit = 0.25m;
            var editSession = os.CreateEditSession();
            var item = editSession.SplitOrderCookingItem(amountToSplit, order, orderItem);
            var guest = order.Guests.FirstOrDefault(p => p != orderItem.Guest);
            if (guest != null)
                editSession.MoveOrderItemToAnotherGuest(item, guest, order);

            os.SubmitChanges(editSession);
        }

        /// <summary>
        /// Сервисная печать последнего блюда заказа.
        /// </summary>
        private static void PrintProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var itemsToPrint = new[] { order.Items.OfType<IOrderCookingItem>().Last() };
            os.PrintOrderItems(os.GetCredentials(), order, itemsToPrint);
        }

        /// <summary>
        /// Сервисная печать заказа.
        /// </summary>
        private static void PrintOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var itemsToPrint = order.Items.OfType<IOrderCookingItem>().ToList();
            os.PrintOrderItems(os.GetCredentials(), order, itemsToPrint);
        }

        /// <summary>
        /// Печать пречека.
        /// </summary>
        private static void PrintBillCheque([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.PrintBillCheque(os.GetCredentials(), order);
        }

        /// <summary>
        /// Печать пречека.
        /// </summary>
        private static void BillOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.BillOrder(os.GetCredentials(), order, 32);
        }

        /// <summary>
        /// Отмена пречека.
        /// </summary>
        private static void CancelBill([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.CancelBill(os.GetCredentials(), order);
        }

        /// <summary>
        /// Удаление последнего гостя заказа.
        /// </summary>
        private static void DeleteGuest([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var guest = order.Guests.Last();
            os.DeleteOrderGuest(order, guest, os.GetCredentials());
        }

        /// <summary>
        /// Удаление последнего блюда из заказа.
        /// </summary>
        private static void DeleteProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.Last();
            os.DeleteOrderItem(order, orderItem, os.GetCredentials());
        }

        /// <summary>
        /// Удаление отпечатанного блюда из заказа.
        /// </summary>
        private static void DeletePrintedProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            var orderItem = order.Items.Last();
            var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last();
            const string comment = "Причина списания продукта";
            var itemsToDelete = new List<IOrderRootItem> { orderItem };

            if (removalType.WriteoffType.Equals(WriteoffType.None))
            {
                os.DeletePrintedOrderItems(comment, WriteoffOptions.WithoutWriteoff(removalType), order, itemsToDelete, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Cafe))
            {
                os.DeletePrintedOrderItems(comment, WriteoffOptions.WriteoffToCafe(removalType), order, itemsToDelete, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Waiter))
            {
                os.DeletePrintedOrderItems(comment, WriteoffOptions.WriteoffToWaiter(removalType), order, itemsToDelete, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.User))
            {
                var user = PluginContext.Operations.GetUsers().First(u => u.IsSessionOpen);
                os.DeletePrintedOrderItems(comment, WriteoffOptions.WriteoffToUser(removalType, user), order, itemsToDelete, credentials);
                return;
            }
            throw new NotSupportedException($"Write-off type '{removalType.WriteoffType}' is not supported.");
        }

        /// <summary>
        /// Удаление модификатора из заказа.
        /// </summary>
        private static void DeleteModifier([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var orderItemModifier = orderItem.AssignedModifiers.Last();
            os.DeleteOrderModifierItem(order, orderItem, orderItemModifier, os.GetCredentials());
        }

        /// <summary>
        /// Удаление отпечатанного модификатора из заказа.
        /// </summary>
        private static void DeletePrintedModifier([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var orderItemModifier = orderItem.AssignedModifiers.Last();
            var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last();
            const string comment = "Причина списания модификатора";

            if (removalType.WriteoffType.Equals(WriteoffType.None))
            {
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, orderItem, orderItemModifier, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Cafe))
            {
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToCafe(removalType), order, orderItem, orderItemModifier, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Waiter))
            {
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToWaiter(removalType), order, orderItem, orderItemModifier, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.User))
            {
                var user = PluginContext.Operations.GetUsers().First(u => u.IsSessionOpen);
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToUser(removalType, user), order, orderItem, orderItemModifier, credentials);
                return;
            }
            throw new NotSupportedException($"Write-off type '{removalType.WriteoffType}' is not supported.");
        }

        /// <summary>
        /// Добавление комментария к блюду.
        /// </summary>
        private static void AddProductComment([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.Status == OrderItemStatus.Added);
            os.ChangeOrderItemComment("Приготовить без соли.", order, orderItem, os.GetCredentials());
        }

        /// <summary>
        /// Удаление комментария к блюду.
        /// </summary>
        private static void DeleteProductComment([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.Status == OrderItemStatus.Added);
            os.DeleteOrderItemComment(order, orderItem, os.GetCredentials());
        }

        /// <summary>
        /// Создание доставки. 
        /// </summary>
        private void CreateDelivery()
        {
            CreateDelivery(false);
        }

        /// <summary>
        /// Создание доставки с ключом источника.
        /// </summary>
        private void CreateDeliveryWithOriginName()
        {
            CreateDelivery(true);
        }

        private const string DeliveryOriginName = "deliveryClub";

        private IDeliveryOrder CreateDelivery(bool withOriginName)
        {
            var editSession = PluginContext.Operations.CreateEditSession();
            var street = PluginContext.Operations.SearchStreets(string.Empty).Last();
            var region = PluginContext.Operations.GetRegions().LastOrDefault();
            var deliveryOperator = PluginContext.Operations.GetUsers().Last();
            var address = new AddressDto
            {
                StreetId = street.Id,
                RegionId = region?.Id ?? Guid.Empty,
                House = "428-с",
                Building = "29-m",
                Flat = "37",
            };

            var orderType = PluginContext.Operations.GetOrderTypes().Last(type => type.OrderServiceType == OrderServiceTypes.DeliveryByCourier);
            // Создание заказа доставки можно использовать только в паре с CreateDelivery
            // в рамках одной EditSession.

            var primaryPhone = new PhoneDto
            {
                PhoneValue = "+79991112233",
                IsMain = true
            };

            var secondaryPhone = new PhoneDto
            {
                PhoneValue = "+79991112244",
                IsMain = false
            };

            var primaryEmail = new EmailDto
            {
                EmailValue = "MyMail@iiko.ru",
                IsMain = true
            };

            var secondaryEmail = new EmailDto
            {
                EmailValue = "MySecondMail@iiko.ru",
                IsMain = false
            };

            var now = DateTime.Now;
            var client = editSession.CreateClient(Guid.NewGuid(), "Semen", new List<PhoneDto> { primaryPhone, secondaryPhone }, null, now);
            var frontDeliverySettings = PluginContext.Operations.GetFrontDeliverySettings();
            var duration = frontDeliverySettings.CourierDeliveryDuration?.TotalMinutes ?? 60;
            var expectedDeliverTime = now.AddMinutes(duration);
            var deliveryOrder = editSession.CreateDeliveryOrder(1149, now, primaryPhone.PhoneValue, address, expectedDeliverTime, orderType, client, deliveryOperator, TimeSpan.FromMinutes(duration));
            if (withOriginName)
                editSession.ChangeOrderOriginName(DeliveryOriginName, deliveryOrder);
            var guest = editSession.AddOrderGuest("Alex", deliveryOrder);
            var product = GetProduct();
            var productSize = product.Scale == null ? null : product.Scale.DefaultSize ?? PluginContext.Operations.GetProductScaleSizes(product.Scale).First();
            editSession.AddOrderProductItem(1m, product, deliveryOrder, guest, productSize);
            editSession.ChangeClientSurname("Petrov", client);
            editSession.ChangeClientNick("Batman", client);
            editSession.ChangeClientEmails(new List<EmailDto> { primaryEmail, secondaryEmail }, client);
            editSession.ChangeClientComment("Customer's Comment", client);
            editSession.ChangeClientAddresses(new List<AddressDto> { address }, 0, client);
            editSession.ChangeClientCardNumber("123456798", client);
            editSession.ChangeDeliveryComment("Delivery comment", deliveryOrder);
            editSession.ChangeDeliveryEmail(primaryEmail.EmailValue, deliveryOrder);

            var entities = PluginContext.Operations.SubmitChanges(editSession);
            return entities.Get(deliveryOrder);
        }

        private void CreateAndSendDelivery()
        {
            if (PluginContext.Operations.GetHostDeliverySettings().DeliveryPaymentTimeOption == DeliveryPaymentTimeOption.BeforeSending)
            {
                var answer = MessageBox.Show(window, "Payment time is set to BeforeSending, but delivery payment is not supported in API yet. Try it anyway?", "SamplePlugin", MessageBoxButton.YesNo);
                if (answer == MessageBoxResult.No)
                    return;
            }

            var credentials = PluginContext.Operations.GetCredentials();

            var delivery = CreateDelivery(false);
            if (PluginContext.Operations.IsDeliveryConfirmationActive())
            {
                Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.Unconfirmed);
                PluginContext.Operations.ChangeDeliveryConfirmTime(DateTime.Now, delivery, credentials);
                delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            }

            PluginContext.Operations.PrintOrderItems(credentials, delivery, delivery.Items.OfType<IOrderCookingItem>().ToList());

            Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.New);
            PluginContext.Operations.PrepareDeliveryForSending(credentials, delivery);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.Waiting);

            var courier = PluginContext.Operations.GetUsers().Single(x => x.Name.Contains("FirstMa"));
            Debug.Assert(delivery.Courier == null);
            PluginContext.Operations.ChangeDeliveryCourier(true, delivery, courier, credentials);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(Equals(delivery.Courier, courier));

            Debug.Assert(!delivery.IsPrintedBillActual);
            PluginContext.Operations.PrintDeliveryBill(credentials, delivery);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(delivery.IsPrintedBillActual);

            PluginContext.Operations.SendDelivery(credentials, delivery);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.OnWay);
        }

        private void SetDeliveryDelivered()
        {
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryDelivered(order);
            editSession.ChangeDeliveryActualDeliverTime(DateTime.Now, order);
            PluginContext.Operations.SubmitChanges(credentials, editSession);
        }

        private void SetDeliveryUndelivered()
        {
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryUndelivered(order);
            editSession.ChangeDeliveryActualDeliverTime(null, order);
            PluginContext.Operations.SubmitChanges(credentials, editSession);
        }

        private void SetDeliveryClosed()
        {
            var now = DateTime.Now;
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryCloseTime(now, order);
            if (order.Client != null)
                editSession.ChangeClientLastOrderDate(now, order.Client);
            PluginContext.Operations.SubmitChanges(credentials, editSession);
        }

        private void SetDeliveryUnclosed()
        {
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryCloseTime(null, order);
            PluginContext.Operations.SubmitChanges(credentials, editSession);
        }

        /// <summary>
        /// Показать доставки с непустым ключом источника
        /// </summary>
        private void ShowDeliveriesWithNotEmptyOriginName()
        {
            var deliveriesWithKey = PluginContext.Operations.GetDeliveryOrders(true)
                .Where(d => d.OriginName != null)
                .Select(d => $"{d.Number}: {d.OriginName}");

            var message = string.Join(",", deliveriesWithKey);
            MessageBox.Show(message);
        }

        /// <summary>
        /// Изменение типа созданной доставки с курьерской на самовывозную.
        /// </summary>
        private void ChangeDeliveryOrderTypeOnSelfService()
        {
            var createdDeliveryByPlugin = PluginContext.Operations.GetDeliveryOrders().LastOrDefault(d => d.DeliveryStatus == DeliveryStatus.New && d.OrderType?.OrderServiceType == OrderServiceTypes.DeliveryByCourier);
            if (createdDeliveryByPlugin == null)
            {
                MessageBox.Show("Please, first call Create Delivery.");
                return;
            }

            var orderType = PluginContext.Operations.GetOrderTypes().First(t => t.OrderServiceType == OrderServiceTypes.DeliveryByClient);

            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetOrderType(orderType, createdDeliveryByPlugin);
            editSession.ChangeDeliveryCourier(false, createdDeliveryByPlugin, null);
            editSession.ChangeDeliveryAddress(null, createdDeliveryByPlugin);

            PluginContext.Operations.SubmitChanges(editSession);
        }

        /// <summary>
        /// Изменение типа созданной доставки с самовывозной на курьерскую.
        /// </summary>
        private void ChangeDeliveryOrderTypeOnCourier()
        {
            var createdDeliveryByPlugin = PluginContext.Operations.GetDeliveryOrders().LastOrDefault(d => d.DeliveryStatus == DeliveryStatus.New && d.OrderType?.OrderServiceType == OrderServiceTypes.DeliveryByClient);
            if (createdDeliveryByPlugin == null)
            {
                MessageBox.Show("Please, first call Create Delivery.");
                return;
            }

            var orderType = PluginContext.Operations.GetOrderTypes().First(t => t.OrderServiceType == OrderServiceTypes.DeliveryByCourier);

            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetOrderType(orderType, createdDeliveryByPlugin);
            editSession.ChangeDeliveryCourier(false, createdDeliveryByPlugin, null);

            var street = PluginContext.Operations.SearchStreets(string.Empty).Last();
            var region = PluginContext.Operations.GetRegions().LastOrDefault();
            var address = new AddressDto
            {
                StreetId = street.Id,
                RegionId = region?.Id ?? Guid.Empty,
                House = "428-с",
                Building = "29-m",
                Flat = "37",
            };

            editSession.ChangeDeliveryAddress(address, createdDeliveryByPlugin);
            PluginContext.Operations.SubmitChanges(editSession);
        }

        /// <summary>
        /// Изменение комментария созданной доставки.
        /// </summary>
        private void ChangeDeliveryComment()
        {
            var createdDeliveryByPlugin = PluginContext.Operations.GetDeliveryOrders().FirstOrDefault(d => d.Number == 1149 && d.DeliveryStatus == DeliveryStatus.New);
            if (createdDeliveryByPlugin == null)
            {
                MessageBox.Show("Please, first call Create Delivery.");
                return;
            }

            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.ChangeDeliveryComment(createdDeliveryByPlugin.Comment + "+Changed by test plugin", createdDeliveryByPlugin);

            PluginContext.Operations.SubmitChanges(editSession);
        }

        private void SplitOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var result = PluginContext.Operations.NeedToSplitOrderBeforePayment(order).CheckSplitRequiredResult;
            if (result == CheckSplitRequiredResult.Disabled)
                return;
            if (result == CheckSplitRequiredResult.Allowed && MessageBox.Show("Split Order by cooking place types?", "Split Orders", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            PluginContext.Operations.SplitOrderBetweenCashRegisters(credentials, order);
        }

        /// <summary>
        /// Добавление скидки.
        /// </summary>
        private void AddDiscount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var discountType = os.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && !x.DiscountByFlexibleSum);
            os.AddDiscount(discountType, order, os.GetCredentials());
        }

        /// <summary>
        /// Добавление скидки на произвольную сумму.
        /// </summary>
        private void AddFlexibleSumDiscount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var discountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.DiscountByFlexibleSum);
            os.AddFlexibleSumDiscount(50, discountType, order, os.GetCredentials());
        }

        /// <summary>
        /// Создание клубной карты с номером "0123456789" и скидкой.
        /// </summary>
        private void CreateDiscountCard()
        {
            var cardDiscountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.CanApplyByDiscountCard);
            PluginContext.Operations.CreateDiscountCard("0123456789", "Semen", null, cardDiscountType);
        }

        /// <summary>
        /// Редактирование клубной карты по номеру "0123456789".
        /// </summary>
        private void UpdateDiscountCard()
        {
            var priceCategory = PluginContext.Operations.GetPriceCategories().Last(x => !x.Deleted);
            const string cardNumber = "0123456789";
            var existingCard = PluginContext.Operations.SearchDiscountCardByNumber(cardNumber);
            if (existingCard == null)
            {
                PluginContext.Log.Warn($"Discount card with number {cardNumber} doesn't exist.");
                return;
            }

            var updatedCard = PluginContext.Operations.UpdateDiscountCard(existingCard.Id, "Mike", priceCategory, null);
            PluginContext.Log.Info($"Updated card owner from {existingCard.OwnerName} to {updatedCard.OwnerName}.");
        }

        /// <summary>
        /// Добавление скидки и связанного с ним гостя.
        /// </summary>
        private void AddDiscountByCardNumber([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var discountCard = os.GetDiscountCards().Last(x => x.DiscountType != null && x.DiscountType.CanApplyByCardNumber);
            os.AddDiscountByCardNumber(discountCard.CardNumber, order, discountCard, os.GetCredentials());
        }

        /// <summary>
        /// Удаление скидки и связанного с ним гостя.
        /// </summary>
        private void DeleteDiscount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var cardDiscountItem = order.Discounts.Last(discount => discount.DiscountType.CanApplyByDiscountCard);
            os.DeleteDiscount(cardDiscountItem, order, os.GetCredentials());
        }

        /// <summary>
        /// Добавление скидки с возможностью выбора блюд.
        /// </summary>
        private void AddSelectiveDiscount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var selectedDish = new List<IOrderProductItemStub> { order.Items.OfType<IOrderProductItem>().Last() };
            var discountType = os.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.CanApplySelectively);

            var editSession = os.CreateEditSession();
            editSession.AddDiscount(discountType, order);
            editSession.ChangeSelectiveDiscount(order, discountType, selectedDish, null, null);
            os.SubmitChanges(editSession);
        }

        /// <summary>
        /// Добавление комбо в заказ.
        /// </summary>
        private void AddComboInOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var editSession = os.CreateEditSession();

            var guest = order.Guests.Last();
            var compoundItem = AddSplittedCompoundItemInternal(order, editSession);
            var product = GetProduct();
            IProductSize size = null;
            if (product.Scale != null)
                size = product.Scale.DefaultSize;

            var productItem = editSession.AddOrderProductItem(1, product, order, guest, size);
            var firstComboGroupId = Guid.NewGuid();
            var secondComboGroupId = Guid.NewGuid();
            var comboItems = new Dictionary<Guid, IOrderCookingItemStub>
            {
                { firstComboGroupId, productItem },
                { secondComboGroupId, compoundItem}
            };

            // идентификаторы связанных с комбо акции и программы лояльности, полученные из внешней системы лояльности (iikocard)
            // здесь заглушки в виде Guid.Empty, поскольку интеграция с внешней системой лояльности выходит за рамки данного примера
            var (sourceActionId, programId) = (Guid.Empty, Guid.Empty);

            editSession.AddOrderCombo(Guid.NewGuid(), "Случайное комбо", 1, 500, sourceActionId, programId, comboItems, order, guest);
            os.SubmitChanges(editSession);
        }

        /// <summary>
        /// Добавление дополнительной информации в заказ.
        /// </summary>
        private static void AddExternalDataToOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.AddOrderExternalData(PluginName, "Sample plugin external data", false, order, os.GetCredentials());

            var value = os.TryGetOrderExternalDataByKey(order, PluginName);
            MessageBox.Show($"Sample plugin external data value: {value}");
        }

        /// <summary>
        /// Удаление дополнительной информации из заказа.
        /// </summary>
        private static void DeleteExternalDataFromOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.DeleteOrderExternalData(PluginName, order, os.GetCredentials());
        }

        /// <summary>
        /// Отменить новую доставку.
        /// </summary>
        private void CancelNewDelivery()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var cancelCause = PluginContext.Operations.GetDeliveryCancelCauses().First();
            PluginContext.Operations.CancelNewDelivery(credentials, order, cancelCause, null);
        }

        /// <summary>
        /// Удалить заказ и не сохранить его элементы в OLAP как удаленные.
        /// </summary>
        private void DeleteOrderAndHideItemsFromOlap()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders(excludeDeliveryOrders: true).Last(o => o.Status == OrderStatus.New);
            var printedItems = order.Items.Where(i => i.PrintTime.HasValue && !i.Deleted).ToList();

            if (printedItems.Count > 0)
            {
                var editSession = PluginContext.Operations.CreateEditSession();
                var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last(rt => rt.WriteoffType.Equals(WriteoffType.None));
                const string comment = "Test API DeleteOrderAndHideItemsFromOlap";

                foreach (var item in printedItems)
                    editSession.DeletePrintedOrderItems(comment, WriteoffOptions.WithoutWriteoff(removalType), order, new List<IOrderRootItem> { item });

                PluginContext.Operations.SubmitChanges(editSession);
                order = PluginContext.Operations.GetOrderById(order.Id);
            }
            PluginContext.Operations.DeleteOrderAndHideItemsFromOlap(credentials, order);
        }

        /// <summary>
        /// Бесследно удалить доставку.
        /// </summary>
        private void CancelNewDeliveryAndHideItemsFromOlap()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var printedItems = order.Items.Where(i => i.PrintTime.HasValue && !i.Deleted).ToList();

            if (printedItems.Count > 0)
            {
                var editSession = PluginContext.Operations.CreateEditSession();
                var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last(rt => rt.WriteoffType.Equals(WriteoffType.None));
                const string comment = "Test API PermanentRemoveDelivery";

                foreach (var item in printedItems)
                    editSession.DeletePrintedOrderItems(comment, WriteoffOptions.WithoutWriteoff(removalType), order, new List<IOrderRootItem> { item });

                PluginContext.Operations.SubmitChanges(editSession);
                order = PluginContext.Operations.GetDeliveryOrderById(order.Id);
            }
            var cancelCause = PluginContext.Operations.GetDeliveryCancelCauses().First();
            PluginContext.Operations.CancelNewDeliveryAndHideItemsFromOlap(credentials, order, cancelCause);
        }

        /// <summary>
        /// Распечатать произвольный текст на одном из принтеров
        /// </summary>
        private void PrintCheck()
        {
            var listView = new ItemsControl();

            var printerSelectionWindow = new Window
            {
                Title = "Select printer",
                Content = listView,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = window
            };

            var clickHandler = new RoutedEventHandler((sender, args) =>
            {
                var button = (Button)sender;
                var selectedPrinter = (IPrinterRef)button.DataContext;

                var doc = new Document
                {
                    Markup = new XElement(Tags.Doc, new XElement(Tags.Center, $"{button.Content} test"))
                };

                if (PluginContext.Operations.Print(selectedPrinter, doc))
                    MessageBox.Show("Document successfully printed", "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.None);
                else
                    MessageBox.Show("Print error", "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);

                printerSelectionWindow.Close();
            });

            var printers = new Dictionary<string, IPrinterRef>
            {
                { "Bill printer", PluginContext.Operations.TryGetBillPrinter(checkIsConfigured: true) },
                { "Document printer", PluginContext.Operations.TryGetDocumentPrinter(checkIsConfigured: true) },
                { "Sticker printer", PluginContext.Operations.TryGetStickerPrinter(checkIsConfigured: true) },
                { "Receipt printer", PluginContext.Operations.TryGetReceiptChequePrinter() },
                { "Report printer", PluginContext.Operations.GetReportPrinter() },
                { "Dish printer", PluginContext.Operations.TryGetDishPrinter(checkIsConfigured: true) }
            };

            foreach (var p in printers.Where(p => p.Value != null))
            {
                var button = new Button
                {
                    DataContext = p.Value,
                    Content = p.Key,
                    Margin = new Thickness(4),
                    Padding = new Thickness(4),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                button.Click += clickHandler;
                listView.Items.Add(button);
            }

            printerSelectionWindow.ShowDialog();
        }

        private void CreateJournalEvents()
        {
            var editSession = PluginContext.Operations.CreateEditSession();
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last();
            var journalEventHigh = editSession.CreateJournalEvent("SamplePlugin", Severity.High, "Sample of high severity journal event", DateTime.Now);
            editSession.AttachToJournalEvent(deliveryOrder, journalEventHigh);
            editSession.SetJournalEventAttribute("comment", $"This is an example of journal event with high severity for delivery {deliveryOrder.Number}", journalEventHigh);

            var journalEventMiddle = editSession.CreateJournalEvent("SamplePlugin", Severity.Middle, "Sample of  middle severity journal event", DateTime.Now);
            editSession.AttachToJournalEvent(deliveryOrder, journalEventMiddle);
            editSession.SetJournalEventAttribute("comment", $"This is an example of journal event with middle severity for delivery {deliveryOrder.Number}", journalEventMiddle);

            var journalEventLow = editSession.CreateJournalEvent("SamplePlugin", Severity.Low, "Sample of low severity journal event", DateTime.Now);
            editSession.AttachToJournalEvent(deliveryOrder, journalEventLow);
            editSession.SetJournalEventAttribute("comment", $"This is an example of journal event with low severity for delivery {deliveryOrder.Number}", journalEventLow);
            PluginContext.Operations.SubmitChanges(editSession);
        }

        private void AddProductAndSetOpenPrice([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var openPriceDish = os.GetActiveProducts().Last(i => i.Type != ProductType.ForPurchase && i.CanSetOpenPrice);

            var editSession = os.CreateEditSession();
            var guest = editSession.AddOrderGuest("Alex", order);
            var product = editSession.AddOrderProductItem(1, openPriceDish, order, guest, null);
            editSession.SetOpenPrice(1000m, product, order);

            os.SubmitChanges(editSession);
        }

        private void ChangePriceCategory([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var priceCategory = os.GetPriceCategories().Last(i => i.CanApplyManually && !i.Deleted);
            os.ChangePriceCategory(priceCategory, order, os.GetCredentials());
        }

        private void ResetPriceCategory([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.ResetPriceCategory(order, os.GetCredentials());
        }

        private static void StartService([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            var serviceProduct = os.GetActiveProducts().Last(x => x.Type == ProductType.Service && x.RateSchedule != null);
            var service = os.AddOrderServiceItem(serviceProduct, order, order.Guests.Last(), credentials, TimeSpan.FromHours(2));
            os.StartService(credentials, order, service);
        }

        private static void StopService([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var service = order.Items.OfType<IOrderServiceItem>().Last(x => x.IsStarted);
            os.StopService(os.GetCredentials(), order, service);
        }

        private static void ChangeOrderTables([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var tables = os.GetTerminalsGroupRestaurantSections(os.GetHostTerminalsGroup()).First().Tables;
            os.ChangeOrderTables(order, new[] { tables.First() }, os.GetCredentials());
        }

        private static void MarkOrderAsTab([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            if (!os.GetHostTerminalsGroup().IsTabMode)
            {
                MessageBox.Show("Marking order as tab supported only in tab mode.", "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var customer = os.TryGetClientById(order.CustomerIds.FirstOrDefault());

            var tabName = customer?.Name ?? $"Kirill {order.Number}";
            os.MarkOrderAsTab(tabName, order, os.GetCredentials());
            order = os.GetOrderById(order.Id);

            PluginContext.Log.Info($"Tab name '{order.TabName}' assigned to the order {order.Number} ({order.Id}).");
        }

        private static void ShowAllergens()
        {
            var lastOrder = PluginContext.Operations.GetOrders().Last();
            var lastOrderItemsCount = lastOrder.Items.Count;

            if (lastOrderItemsCount > 0)
            {
                var lastOrderItem = lastOrder.Items.Last();
                var allergenGroups = PluginContext.Operations.GetAllergenGroupsByOrderRootItem(lastOrderItem);
                var message = "Allergens of the last item of the last order: " + Environment.NewLine +
                              string.Join(",", allergenGroups.Select(ag => ag.Name));
                MessageBox.Show(message);

                if (lastOrderItemsCount > 1)
                {
                    var productIds = lastOrder.Items.OfType<IOrderProductItem>().Select(i => i.Product.Id).ToArray();
                    allergenGroups = PluginContext.Operations.GetAllergenGroupsByProductIds(productIds);
                    message = "Allergens of all IOrderProductItem of the last order: " + Environment.NewLine +
                              string.Join(",", allergenGroups.Select(ag => ag.Name).Distinct());
                    MessageBox.Show(message);
                }
            }
        }

        private void ChangeOrderComment()
        {
            var lastOrder = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.ChangeOrderComment($"comment changed: {DateTime.Now}", lastOrder, PluginContext.Operations.GetCredentials());
        }

        private void AddPublicExternalData()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.AddOrderExternalData(PluginName, "Sample plugin public external data", true, order, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Изменение места приготовления у блюд.
        /// </summary>
        private static void ChangeCookingItemsCookingPlace([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItems = order.Items.OfType<IOrderCookingItem>().Where(product => product.Status == OrderItemStatus.Added).ToList();
            var restaurantSection = os.GetTerminalsGroupRestaurantSections(os.GetHostTerminalsGroup()).Single(rs => rs.Name == "Бар");
            os.ChangeOrderItemsCookingPlace(restaurantSection, order, orderItems, os.GetCredentials());
        }

        private void NavigateToCurrentOrder(IOrder order, IOperationService os, IViewManager vm)
        {
            // Навигация в себя же избыточна и не происходит.
            vm.NavigateToOrderAfterOperation(order);
        }

        private void NavigateToOtherOrder(IOrder order, IOperationService os, IViewManager vm)
        {
            var otherOrder = os.GetOrders().LastOrDefault(o => o.Status == OrderStatus.New && o.Id != order.Id);
            if (otherOrder == null)
                return;

            vm.NavigateToOrderAfterOperation(otherOrder);
        }

        private static void AddOrUpdateKitchenOrderExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var key = "key";
            var value = "value";

            PluginContext.Operations.AddOrUpdateKitchenOrderExternalData(kitchenOrder, key, value);
        }

        private static void DeleteKitchenOrderExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var key = "key";

            PluginContext.Operations.DeleteKitchenOrderExternalData(kitchenOrder, key);
        }

        private static void TryGetByIdKitchenOrderExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var key = "key";

            var value = PluginContext.Operations.TryGetKitchenOrderExternalDataByKey(kitchenOrder, key);
            if (value != null)
                MessageBox.Show($"External data value: {value}");
        }

        private void GetPastOrderById()
        {
            var orderId = new Guid("8BA03A53-3A06-441A-92A2-0FBA1F5E79F1");
            var pastOrder = PluginContext.Operations.GetPastOrder(orderId);
            MessageBox.Show(pastOrder.Number.ToString(), "Past order");
        }

        private void GetPastOrdersBySum()
        {
            var paymentSum = 415m;
            var minCloseTime = new DateTime(2022, 2, 2);
            var maxCloseTime = new DateTime(2022, 2, 20);
            var pastOrders = PluginContext.Operations.GetPastOrdersBySum(paymentSum, minCloseTime, maxCloseTime);
            var resultString = pastOrders.Aggregate(string.Empty, (current, order) => current + order.Number + Environment.NewLine);
            MessageBox.Show(resultString, "Past orders");
        }

        private void GetPastOrdersByNumber()
        {
            var orderNumber = 15;
            var minCloseTime = new DateTime(2022, 2, 2);
            var maxCloseTime = new DateTime(2022, 2, 20);
            var pastOrders = PluginContext.Operations.GetPastOrders(orderNumber, minCloseTime, maxCloseTime);
            var resultString = pastOrders.Aggregate(string.Empty, (current, order) => current + order.Number + Environment.NewLine);
            MessageBox.Show(resultString, "Past orders");
        }

        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
        }
    }
}
