using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using Resto.Front.Api.Data.Kitchen;
using Resto.Front.Api.Data.Print;
using Resto.Front.Api.Editors.Stubs;
using Resto.Front.Api.UI;
using Button = System.Windows.Controls.Button;
using Resto.Front.Api.Data.View;

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
            AddButton("Add or update modifier for printed item", AddOrUpdateModifierForPrintedItem);
            AddButton("Delete printed modifier", DeletePrintedModifier);
            AddButton("Add product comment", AddProductComment);
            AddButton("Delete product comment", DeleteProductComment);
            AddButton("Delete printed product comment", DeletePrintedProductComment);
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
            AddButton("Try get order external data", TryGetByKeyOrderExternalData);
            AddButton("Get all external data of the order", GetOrderAllExternalData);
            AddButton("Get all public external data of the order", GetOrderPublicExternalData);
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
            AddButton("Navigate to order", NavigateToOrder);
            AddButton("Add or update kitchen order external data", AddOrUpdateKitchenOrderExternalData);
            AddButton("Delete external data from kitchen order", DeleteKitchenOrderExternalData);
            AddButton("Try get kitchen order external data", TryGetByKeyKitchenOrderExternalData);
            AddButton("Get all external data of the kitchen order", GetKitchenOrderAllExternalData);
            AddButton("Get all public external data of the kitchen order", GetKitchenOrderPublicExternalData);
            AddButton("Add or update kitchen order item external data", AddOrUpdateKitchenOrderItemExternalData);
            AddButton("Delete external data from kitchen order item", DeleteKitchenOrderItemExternalData);
            AddButton("Try get kitchen order item external data", TryGetByKeyKitchenOrderItemExternalData);
            AddButton("Get all external data of the kitchen order item", GetKitchenOrderItemAllExternalData);
            AddButton("Get all public external data of the kitchen order item", GetKitchenOrderItemPublicExternalData);
            AddButton("Get past order by id", GetPastOrderById);
            AddButton("Get past orders by sum", GetPastOrdersBySum);
            AddButton("Get past orders by number", GetPastOrdersByNumber);
            AddButton("Get local terminal`s restaurant sections", GetHostTerminalRestaurantSections);
            AddButton("Change selective discount", ChangeSelectiveFlexibleSumDiscount);
            AddButton("Add product without size to stop list", AddProductWithoutSizeToStopList);
            AddButton("Add product with size to stop list", AddProductWithSizeToStopList);
            AddButton("Set stop list product remaining amount", SetStopListProductRemainingAmount);
            AddButton("Clear stop list", ClearStopList);
            AddButton("Remove product from stop list", RemoveProductFromStopList);
            AddButton("Get stop list products remaining amounts", GetStopListProductsRemainingAmounts);
            AddButton("Change marking code", ChangeMarkingCode);
            AddButton("Check is product selling restricted", IsInStopList);
            AddButton("Check products selling restrictions", CheckProductsSellingRestrictions);
            AddButton("Create kitchen order", CreateKitchenOrder);
            AddButton("Create empty kitchen order", CreateEmptyKitchenOrder);
            AddButton("Delete kitchen order items", DeleteKitchenOrderItems);
            AddButton("Print on printing device", Print);
            AddButton("Show custom data dialog window", ShowCustomDataDialogWindow);
            AddButton("Check local terminal connection to the main terminal", IsConnectedToMainTerminal);
            AddButton("Get user`s full name", GetUserFullName);
            AddButton("Get address settings", GetAddressSettings);

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

        private void AddButton(string text, Action<IViewManager> action)
        {
            AddButton(text, () =>
            {
                PluginContext.Operations.TryExecuteUiOperation(action);
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
            var credentials = PluginContext.Operations.GetDefaultCredentials();
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
            var credentials = PluginContext.Operations.GetDefaultCredentials();
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
            var credentials = os.GetDefaultCredentials();
            os.AddOrderGuest("John Doe", order, credentials);
        }

        /// <summary>
        /// Привязать гостя Semen с номером карты "0123456789" к заказу.
        /// </summary>
        private static void AddClientToOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetDefaultCredentials();
            var discountType = os.GetDiscountTypes().LastOrDefault(d => d.CanApplyByDiscountCard);
            const string cardNumber = "0123456789";
            const string clientName = "Semen";

            var client = os.CreateClient(Guid.NewGuid(), clientName, null, cardNumber, DateTime.Now, credentials);

            var card = os.SearchDiscountCardByNumber(cardNumber);
            if (card == null)
                os.CreateDiscountCard(cardNumber, clientName, null, discountType);
            else
                os.UpdateDiscountCard(card.Id, clientName, null, discountType);

            os.AddClientToOrder(order, client, credentials);
        }

        /// <summary>
        /// Отвязать гостя Semen от заказа.
        /// </summary>
        private static void RemoveOrderClient([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetDefaultCredentials();
            var client = os.SearchClients("Semen", credentials).Last();
            os.RemoveOrderClient(order, client, credentials);
        }

        /// <summary>
        /// Добавление продукта из номенклатуры.
        /// </summary>
        private void AddProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetDefaultCredentials();
            var product = GetProduct();
            var guest = order.Guests.Last();
            var size = product.Scale?.DefaultSize;
            var editSession = os.CreateEditSession();
            var amount = 1;
            var stub = editSession.AddOrderProductItem(amount, product, order, guest, size);
            var simpleModifiers = product.GetSimpleModifiers(null).Where(x => x.DefaultAmount != 0);
            foreach (var item in simpleModifiers)
            {
                editSession.AddOrderModifierItem(item.DefaultAmount, item.Product, null, order, stub);
            }
            var groupModifiers = product.GetGroupModifiers(null);
            foreach (var item in groupModifiers)
            {
                var subItemsToAdd = item.Items.Where(x => x.DefaultAmount != 0);
                foreach (var subItem in subItemsToAdd)
                {
                    editSession.AddOrderModifierItem(subItem.DefaultAmount, subItem.Product, item.ProductGroup, order, stub);
                }
            }
            var createdEntities = os.SubmitChanges(editSession, credentials);
            var addedItem = createdEntities.Get(stub);

            if (product.ImmediateCookingStart)
                os.PrintOrderItems(os.GetOrderById(order.Id), new[] { addedItem }, credentials);
        }

        private IProduct GetProduct(bool isCompound = false, string number = "")
        {
            var activeProducts = PluginContext.Operations.GetActiveProducts()
                .Where(product =>
                    isCompound
                        ? product.Template != null
                        : product.Template == null
                        && product.Type == ProductType.Dish)
                .ToList();

            var index = rnd.Next(activeProducts.Count);
            return number != "" ? activeProducts.FirstOrDefault(p => p.Number == number) : activeProducts[index];
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
            var size = template.Scale?.DefaultSize;
            var editSession = os.CreateEditSession();
            var compoundItem = editSession.AddOrderCompoundItem(product, order, guest, size);
            var primaryComponent = editSession.AddPrimaryComponent(product, order, compoundItem);
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
            os.AddOrderModifierItem(1, modifier.Product, null, order, orderItem, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Добавление группового модификатора.
        /// </summary>
        private static void AddGroupModifier([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var groupModifier = orderItem.AvailableGroupModifiers.Last();
            os.AddOrderModifierItem(1, groupModifier.Items.Last().Product, groupModifier.ProductGroup, order, orderItem, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Увеличение количества последнего блюда заказа на 1.
        /// </summary>
        private static void IncreaseAmount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            os.ChangeOrderCookingItemAmount(orderItem.Amount + 1, orderItem, order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Уменьшение количества последнего блюда заказа на 1.
        /// </summary>
        private static void DecreaseAmount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            os.ChangeOrderCookingItemAmount(orderItem.Amount - 1, orderItem, order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Задание количества последнего блюда заказа.
        /// </summary>
        private static void SetAmount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            var amount = 1.1m;
            os.ChangeOrderCookingItemAmount(amount, orderItem, order, os.GetDefaultCredentials());
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
            os.ChangeOrderWaiter(order, selectedUser, os.GetDefaultCredentials());
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
            os.PrintOrderItems(order, itemsToPrint, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Сервисная печать заказа.
        /// </summary>
        private static void PrintOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var itemsToPrint = order.Items.OfType<IOrderCookingItem>().ToList();
            os.PrintOrderItems(order, itemsToPrint, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Печать пречека.
        /// </summary>
        private static void PrintBillCheque([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.PrintBillCheque(order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Печать пречека. 
        /// </summary>
        private static void BillOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.BillOrder(order, 32, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Отмена пречека.
        /// </summary>
        private static void CancelBill([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.CancelBill(order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление последнего гостя заказа.
        /// </summary>
        private static void DeleteGuest([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var guest = order.Guests.Last();
            os.DeleteOrderGuest(order, guest, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление последнего блюда из заказа.
        /// </summary>
        private static void DeleteProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.Last();
            os.DeleteOrderItem(order, orderItem, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление отпечатанного блюда из заказа.
        /// </summary>
        private static void DeletePrintedProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetDefaultCredentials();
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
            os.DeleteOrderModifierItem(order, orderItem, orderItemModifier, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление отпечатанного модификатора из заказа.
        /// </summary>
        private static void DeletePrintedModifier([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetDefaultCredentials();
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
        /// Добавление\обновление модификатора для отпечатанного блюда.
        /// </summary>
        private static void AddOrUpdateModifierForPrintedItem([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            const int amount = 1;
            const int payableAmount = 1;
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            if (orderItem.AvailableGroupModifiers.Any())
            {
                var modifier = orderItem.AvailableGroupModifiers.First();
                os.AddOrUpdateModifierForPrintedItem(amount, order, orderItem, modifier.Items.First().Product, modifier.ProductGroup,
                    os.GetDefaultCredentials(), payableAmount, modifier.Items.First().Product.Price);
            }
            else
            {
                var modifier = orderItem.AvailableSimpleModifiers.First();
                os.AddOrUpdateModifierForPrintedItem(amount, order, orderItem, modifier.Product, null,
                    os.GetDefaultCredentials(), payableAmount, modifier.Product.Price);
            }
        }

        /// <summary>
        /// Добавление комментария к блюду.
        /// </summary>
        private static void AddProductComment([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            os.ChangeOrderItemComment("Приготовить без соли.", order, orderItem, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление комментария к блюду.
        /// </summary>
        private static void DeleteProductComment([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.Status == OrderItemStatus.Added);
            os.DeleteOrderItemComment(order, orderItem, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление комментария к отпечатанному блюду.
        /// </summary>
        private static void DeletePrintedProductComment([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.PrintTime.HasValue);
            os.DeletePrintedOrderItemComment(order, orderItem, os.GetDefaultCredentials());
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

            var credentials = PluginContext.Operations.GetDefaultCredentials();

            var delivery = CreateDelivery(false);
            if (PluginContext.Operations.IsDeliveryConfirmationActive())
            {
                Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.Unconfirmed);
                PluginContext.Operations.ChangeDeliveryConfirmTime(DateTime.Now, delivery, credentials);
                delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            }

            PluginContext.Operations.PrintOrderItems(delivery, delivery.Items.OfType<IOrderCookingItem>().ToList(), credentials);

            Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.New);
            PluginContext.Operations.PrepareDeliveryForSending(delivery, credentials);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.Waiting);

            var courier = PluginContext.Operations.GetUsers().Single(x => x.Name.Contains("FirstMa"));
            Debug.Assert(delivery.Courier == null);
            PluginContext.Operations.ChangeDeliveryCourier(true, delivery, courier, credentials);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(Equals(delivery.Courier, courier));

            Debug.Assert(!delivery.IsPrintedBillActual);
            PluginContext.Operations.PrintDeliveryBill(delivery, credentials);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(delivery.IsPrintedBillActual);

            PluginContext.Operations.SendDelivery(delivery, credentials);
            delivery = PluginContext.Operations.GetDeliveryOrderById(delivery.Id);
            Debug.Assert(delivery.DeliveryStatus == DeliveryStatus.OnWay);
        }

        private void SetDeliveryDelivered()
        {
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryDelivered(order);
            editSession.ChangeDeliveryActualDeliverTime(DateTime.Now, order);
            PluginContext.Operations.SubmitChanges(editSession, credentials);
        }

        private void SetDeliveryUndelivered()
        {
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryUndelivered(order);
            editSession.ChangeDeliveryActualDeliverTime(null, order);
            PluginContext.Operations.SubmitChanges(editSession, credentials);
        }

        private void SetDeliveryClosed()
        {
            var now = DateTime.Now;
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryCloseTime(now, order);
            if (order.Client != null)
                editSession.ChangeClientLastOrderDate(now, order.Client);
            PluginContext.Operations.SubmitChanges(editSession, credentials);
        }

        private void SetDeliveryUnclosed()
        {
            var order = PluginContext.Operations.GetDeliveryOrders().Last();
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.SetDeliveryCloseTime(null, order);
            PluginContext.Operations.SubmitChanges(editSession, credentials);
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
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var result = PluginContext.Operations.NeedToSplitOrderBeforePayment(order).CheckSplitRequiredResult;
            if (result == CheckSplitRequiredResult.Disabled)
                return;
            if (result == CheckSplitRequiredResult.Allowed && MessageBox.Show("Split Order by cooking place types?", "Split Orders", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            PluginContext.Operations.SplitOrderBetweenCashRegisters(order, credentials);
        }

        /// <summary>
        /// Добавление скидки.
        /// </summary>
        private void AddDiscount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var discountType = os.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && !x.DiscountByFlexibleSum);
            os.AddDiscount(discountType, order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Добавление скидки на произвольную сумму.
        /// </summary>
        private void AddFlexibleSumDiscount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var discountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.DiscountByFlexibleSum);
            os.AddFlexibleSumDiscount(50, discountType, order, os.GetDefaultCredentials());
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
            os.AddDiscountByCardNumber(discountCard.CardNumber, order, discountCard, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удаление скидки и связанного с ним гостя.
        /// </summary>
        private void DeleteDiscount([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var cardDiscountItem = order.Discounts.Last(discount => discount.DiscountType.CanApplyByDiscountCard);
            os.DeleteDiscount(cardDiscountItem, order, os.GetDefaultCredentials());
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
            os.AddOrderExternalData(PluginName, "Sample plugin external data", false, order, os.GetDefaultCredentials());

            var value = os.TryGetOrderExternalDataByKey(order, PluginName);
            MessageBox.Show($"Sample plugin external data value: {value}");
        }

        private static void TryGetByKeyOrderExternalData([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var key = "key";

            var value = PluginContext.Operations.TryGetKitchenOrderExternalDataByKey(kitchenOrder, key);
            if (value != null)
                MessageBox.Show($"External data value: {value}");
        }

        private static void GetOrderAllExternalData([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var result = os.GetOrderAllExternalData(order);
            if (!result.IsEmpty())
            {
                var message = string.Empty;
                result.ForEach(extData => message += extData + Environment.NewLine);
                MessageBox.Show(message);
            }
        }

        private static void GetOrderPublicExternalData([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var result = os.GetOrderAllExternalData(order, true);
            if (!result.IsEmpty())
            {
                var message = string.Empty;
                result.ForEach(extData => message += extData + Environment.NewLine);
                MessageBox.Show(message);
            }
        }

        /// <summary>
        /// Удаление дополнительной информации из заказа.
        /// </summary>
        private static void DeleteExternalDataFromOrder([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.DeleteOrderExternalData(PluginName, order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Отменить новую доставку.
        /// </summary>
        private void CancelNewDelivery()
        {
            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var order = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var cancelCause = PluginContext.Operations.GetDeliveryCancelCauses().First();
            PluginContext.Operations.CancelNewDelivery(order, cancelCause, null, credentials);
        }

        /// <summary>
        /// Удалить заказ и не сохранить его элементы в OLAP как удаленные.
        /// </summary>
        private void DeleteOrderAndHideItemsFromOlap()
        {
            var credentials = PluginContext.Operations.GetDefaultCredentials();
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
            PluginContext.Operations.DeleteOrderAndHideItemsFromOlap(order, credentials);
        }

        /// <summary>
        /// Бесследно удалить доставку.
        /// </summary>
        private void CancelNewDeliveryAndHideItemsFromOlap()
        {
            var credentials = PluginContext.Operations.GetDefaultCredentials();
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
            PluginContext.Operations.CancelNewDeliveryAndHideItemsFromOlap(order, cancelCause, credentials);
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
                var selectedPrinter = (IPrinterQueueRef)button.DataContext;

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

            var printers = new Dictionary<string, IPrinterQueueRef>
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
            os.ChangePriceCategory(priceCategory, order, os.GetDefaultCredentials());
        }

        private void ResetPriceCategory([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            os.ResetPriceCategory(order, os.GetDefaultCredentials());
        }

        private static void StartService([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetDefaultCredentials();
            var serviceProduct = os.GetActiveProducts().Last(x => x.Type == ProductType.Service && x.RateSchedule != null);
            var service = os.AddOrderServiceItem(serviceProduct, order, order.Guests.Last(), credentials, TimeSpan.FromHours(2));
            os.StartService(order, service, credentials);
        }

        private static void StopService([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var service = order.Items.OfType<IOrderServiceItem>().Last(x => x.IsStarted);
            os.StopService(order, service, os.GetDefaultCredentials());
        }

        private static void ChangeOrderTables([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var tables = os.GetTerminalsGroupRestaurantSections(os.GetHostTerminalsGroup()).First().Tables;
            os.ChangeOrderTables(order, new[] { tables.First() }, os.GetDefaultCredentials());
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
            os.MarkOrderAsTab(tabName, order, os.GetDefaultCredentials());
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
            PluginContext.Operations.ChangeOrderComment($"comment changed: {DateTime.Now}", lastOrder, PluginContext.Operations.GetDefaultCredentials());
        }

        private void AddPublicExternalData()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.AddOrderExternalData(PluginName, "Sample plugin public external data", true, order, PluginContext.Operations.GetDefaultCredentials());
        }

        /// <summary>
        /// Изменение места приготовления у блюд.
        /// </summary>
        private static void ChangeCookingItemsCookingPlace([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var orderItems = order.Items.OfType<IOrderCookingItem>().Where(product => product.Status == OrderItemStatus.Added).ToList();
            var restaurantSection = os.GetTerminalsGroupRestaurantSections(os.GetHostTerminalsGroup()).Single(rs => rs.Name == "Бар");
            os.ChangeOrderItemsCookingPlace(restaurantSection, order, orderItems, os.GetDefaultCredentials());
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

        private void NavigateToOrder(IViewManager vm)
        {
            var order = PluginContext.Operations.GetOrders().LastOrDefault(o => o.Status == OrderStatus.New);
            if (order == null)
                return;

            vm.NavigateToOrderAfterOperation(order);
        }

        private static void AddOrUpdateKitchenOrderExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var key = "key";
            var value = "value";
            var isPublic = true;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            PluginContext.Operations.AddOrUpdateKitchenOrderExternalData(kitchenOrder, key, value, isPublic);
        }

        private static void AddOrUpdateKitchenOrderItemExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var baseOrderItemId = kitchenOrder.Items.OfType<IKitchenOrderProductItem>().Last().BaseOrderItemId;
            var key = "item";
            var value = "value";
            var isPublic = true;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            PluginContext.Operations.AddOrUpdateKitchenOrderItemExternalData(kitchenOrder, baseOrderItemId, key, value, isPublic);
        }

        private static void DeleteKitchenOrderItemExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var baseOrderItemId = kitchenOrder.Items.OfType<IKitchenOrderProductItem>().Last().BaseOrderItemId;
            var key = "item";

            PluginContext.Operations.DeleteKitchenOrderItemExternalData(kitchenOrder, baseOrderItemId, key);
        }

        private static void TryGetByKeyKitchenOrderItemExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var baseOrderItemId = kitchenOrder.Items.OfType<IKitchenOrderProductItem>().Last().BaseOrderItemId;
            var key = "item";

            var value = PluginContext.Operations.TryGetKitchenOrderItemExternalDataByKey(kitchenOrder, baseOrderItemId, key);
            if (value != null)
                MessageBox.Show($"External data value: {value}");
        }

        private static void GetKitchenOrderItemAllExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var baseOrderItemId = kitchenOrder.Items.OfType<IKitchenOrderProductItem>().Last().BaseOrderItemId;
            var result = PluginContext.Operations.GetKitchenOrderItemAllExternalData(kitchenOrder, baseOrderItemId);
            if (!result.IsEmpty())
            {
                var message = string.Empty;
                result.ForEach(extData => message += extData + Environment.NewLine);
                MessageBox.Show(message);
            }
        }

        private static void GetKitchenOrderItemPublicExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var baseOrderItemId = kitchenOrder.Items.OfType<IKitchenOrderProductItem>().Last().BaseOrderItemId;
            var result = PluginContext.Operations.GetKitchenOrderItemAllExternalData(kitchenOrder, baseOrderItemId, true);
            if (!result.IsEmpty())
            {
                var message = string.Empty;
                result.ForEach(extData => message += extData + Environment.NewLine);
                MessageBox.Show(message);
            }
        }

        private static void DeleteKitchenOrderExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var key = "key";

            PluginContext.Operations.DeleteKitchenOrderExternalData(kitchenOrder, key);
        }

        private static void TryGetByKeyKitchenOrderExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var key = "key";

            var value = PluginContext.Operations.TryGetKitchenOrderExternalDataByKey(kitchenOrder, key);
            if (value != null)
                MessageBox.Show($"External data value: {value}");
        }

        private static void GetKitchenOrderAllExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var result = PluginContext.Operations.GetKitchenOrderAllExternalData(kitchenOrder);
            if (!result.IsEmpty())
            {
                var message = string.Empty;
                result.ForEach(extData => message += extData + Environment.NewLine);
                MessageBox.Show(message);
            }
        }

        private static void GetKitchenOrderPublicExternalData()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last();
            var result = PluginContext.Operations.GetKitchenOrderAllExternalData(kitchenOrder, true);
            if (!result.IsEmpty())
            {
                var message = string.Empty;
                result.ForEach(extData => message += extData + Environment.NewLine);
                MessageBox.Show(message);
            }
        }

        private void GetPastOrderById()
        {
            var orderId = new Guid("8BA03A53-3A06-441A-92A2-0FBA1F5E79F1");
            var pastOrder = PluginContext.Operations.GetPastOrder(orderId);
            MessageBox.Show(pastOrder.Number.ToString(), "Past order");
        }

        private void GetPastOrdersBySum()
        {
            var minimalPaymentSum = 100m;
            var maximalPaymentSum = 300m;
            var minCloseTime = new DateTime(2023, 7, 19);
            var maxCloseTime = new DateTime(2023, 10, 19);
            var pastOrders = PluginContext.Operations.GetPastOrdersBySum(minimalPaymentSum, maximalPaymentSum, minCloseTime, maxCloseTime);
            var resultString = pastOrders.Aggregate(string.Empty, (current, order) => current + order.Number + Environment.NewLine);
            MessageBox.Show(resultString, "Past orders");
        }

        private void GetPastOrdersByNumber()
        {
            var orderNumber = 15;
            var minCloseTime = new DateTime(2023, 7, 19);
            var maxCloseTime = new DateTime(2023, 10, 19);
            var isEndOfOrderNumber = true;
            var pastOrders = PluginContext.Operations.GetPastOrders(orderNumber, minCloseTime, maxCloseTime, isEndOfOrderNumber);
            var resultString = pastOrders.Aggregate(string.Empty, (current, order) => current + order.Number + Environment.NewLine);
            MessageBox.Show(resultString, "Past orders");
        }

        private void GetHostTerminalRestaurantSections()
        {
            var restaurantSections = PluginContext.Operations.GetHostTerminalRestaurantSections();
            var resultString = restaurantSections.Aggregate(string.Empty, (current, section) => current + section.Id + " - " + section.Name + Environment.NewLine);
            MessageBox.Show(resultString, "Restaurant Sections");
        }

        private void ChangeSelectiveFlexibleSumDiscount(IOrder order, IOperationService operations)
        {
            var rand = new Random();
            var itemsIdWithSum = new List<(Guid, decimal)>();

            foreach (var item in order.Items.Where(i => !i.Deleted))
            {
                switch (item)
                {
                    case IOrderProductItem productItem:
                        {
                            itemsIdWithSum.Add((productItem.Id, rand.Next(1, 50)));
                            itemsIdWithSum.AddRange(productItem.AssignedModifiers.Where(m => !m.Deleted).Select(modifier => (modifier.Id, rand.Next(1, 50))).Select(dummy => ((Guid, decimal))dummy));
                            break;
                        }

                    case IOrderServiceItem serviceItem:
                        itemsIdWithSum.Add((serviceItem.Id, rand.Next(1, 50)));
                        break;

                    case IOrderCompoundItem compoundItem:
                        {
                            itemsIdWithSum.AddRange(compoundItem.CommonModifiers.Select(modifier => (modifier.Id, rand.Next(1, 50))).Select(dummy => ((Guid, decimal))dummy));

                            itemsIdWithSum.Add((compoundItem.PrimaryComponent.Id, rand.Next(1, 50)));
                            itemsIdWithSum.AddRange(compoundItem.PrimaryComponent.Modifiers.Where(m => !m.Deleted).Select(modifier => (modifier.Id, rand.Next(1, 50))).Select(dummy => ((Guid, decimal))dummy));

                            if (compoundItem.SecondaryComponent != null)
                            {
                                itemsIdWithSum.Add((compoundItem.SecondaryComponent.Id, rand.Next(1, 50)));
                                itemsIdWithSum.AddRange(compoundItem.SecondaryComponent.Modifiers.Where(m => !m.Deleted).Select(modifier => (modifier.Id, rand.Next(1, 50))).Select(dummy => ((Guid, decimal))dummy));
                            }
                            break;
                        }
                }
            }

            var editSession = operations.CreateEditSession();
            var discountType = PluginContext.Operations.GetDiscountTypes().First(d => d.CanApplySelectively && d.DiscountByFlexibleSum);
            editSession.AddFlexibleSumDiscount(rand.Next(1, 50), discountType, order);
            editSession.ChangeSelectiveDiscount(itemsIdWithSum, order, discountType);
            operations.SubmitChanges(editSession, operations.AuthenticateByPin("777"));
        }

        private void AddProductWithoutSizeToStopList()
        {
            var activeProducts = PluginContext.Operations.GetActiveProducts()
                .Where(product => product.Template == null && product.Type == ProductType.Dish && product.Scale is null)
                .ToList();
            var index = rnd.Next(activeProducts.Count);
            var product = activeProducts[index];

            PluginContext.Operations.AddProductToStopList(product, null, PluginContext.Operations.GetDefaultCredentials());
        }

        private void AddProductWithSizeToStopList()
        {
            var activeProducts = PluginContext.Operations.GetActiveProducts()
                .Where(product => product.Template == null && product.Type == ProductType.Dish && product.Scale is not null)
                .ToList();
            var index = rnd.Next(activeProducts.Count);
            var product = activeProducts[index];

            // Точно знаем, что Scale не null из условия в Where
            var productSizes = PluginContext.Operations.GetProductScaleSizes(product.Scale!);
            var productSize = productSizes[rnd.Next(productSizes.Count)];

            PluginContext.Operations.AddProductToStopList(product, productSize, PluginContext.Operations.GetDefaultCredentials());
        }

        private void GetStopListProductsRemainingAmounts()
        {
            var sb = new System.Text.StringBuilder();
            var productsRemainingAmounts = PluginContext.Operations.GetStopListProductsRemainingAmounts();
            if (productsRemainingAmounts.Count == 0)
                sb.Append("You have no products in stop list, have a good day!");
            else
                productsRemainingAmounts
                    .OrderBy(k => k.Key.Product.Name)
                    .ThenBy(k => k.Key.ProductSize?.Name)
                    .ForEach(e => sb.AppendLine($"{e.Key.Product.Name} (size - {e.Key.ProductSize?.Name ?? "NULL"}) - {e.Value}"));
            MessageBox.Show(sb.ToString(), "Stop list", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChangeMarkingCode([NotNull] IOrder order, [NotNull] IOperationService os, IViewManager vm)
        {
            var orderItems = order.Items;
            var productItems = orderItems.OfType<IOrderProductItem>().ToArray();
            if (!productItems.Any())
            {
                vm.ShowErrorPopup("No active products!");
                return;
            }

            var chooseResult = vm.ShowChooserPopup("Select product", productItems.Select(item => item.Product.Name).ToList());
            if (chooseResult < 0)
                return;

            var dialogResult = vm.ShowExtendedKeyboardDialog("Scan marking code or enter it manually. ", enableBarcode: true);
            string markingCode = null;
            switch (dialogResult)
            {
                case null: // user cancelled the dialog
                    break;
                case BarcodeInputDialogResult barcode:
                    markingCode = barcode.Barcode;
                    break;
                case CardInputDialogResult card:
                    markingCode = card.Track2;
                    break;
                case StringInputDialogResult str:
                    markingCode = str.Result;
                    break;
                default:
                    throw new NotSupportedException(nameof(dialogResult.GetType));
            }

            os.ChangeOrderItemMarkingCode(markingCode, order, productItems[chooseResult], os.GetDefaultCredentials());
        }

        private void ClearStopList()
        {
            PluginContext.Operations.ClearStopList(PluginContext.Operations.GetDefaultCredentials());
        }

        private void RemoveProductFromStopList()
        {
            var products = PluginContext.Operations.GetStopListProductsRemainingAmounts();
            if (products.Count == 0)
                return;

            var productKvpToRemove = products.Last();
            PluginContext.Operations.RemoveProductFromStopList(productKvpToRemove.Key.Product, productKvpToRemove.Key.ProductSize, PluginContext.Operations.GetDefaultCredentials());
        }

        private void SetStopListProductRemainingAmount()
        {
            var product = GetProduct();

            IProductSize productSize = null;
            if (product.Scale != null)
            {
                var productSizes = PluginContext.Operations.GetProductScaleSizes(product.Scale);
                productSize = productSizes[rnd.Next(productSizes.Count)];
            }

            PluginContext.Operations.SetStopListProductRemainingAmount(product, productSize, rnd.Next(1, 50), PluginContext.Operations.GetDefaultCredentials());
        }

        private void IsInStopList()
        {
            var product = GetProduct();
            IProductSize productSize = null;
            if (product.Scale != null)
            {
                var productSizes = PluginContext.Operations.GetProductScaleSizes(product.Scale);
                productSize = productSizes[rnd.Next(productSizes.Count)];
            }
            var restricted = PluginContext.Operations.IsStopListProductSellingRestricted(product, productSize);
            MessageBox.Show(
                $"{product.Name}{(productSize is null ? string.Empty : $" (size - {productSize.Name})")} selling is {(restricted ? "RESTRICTED" : "ALLOWED")}",
                "Check selling restriction", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CheckProductsSellingRestrictions()
        {
            var product = GetProduct();
            IProductSize productSize = null;
            if (product.Scale != null)
            {
                var productSizes = PluginContext.Operations.GetProductScaleSizes(product.Scale);
                productSize = productSizes[rnd.Next(productSizes.Count)];
            }

            var dict = new Dictionary<ProductAndSize, decimal>
            {
                { new ProductAndSize(product, productSize), new Random().Next(1, 50) }
            };
            var restrictions = PluginContext.Operations.CheckStopListProductsSellingRestrictions(dict, PluginContext.Operations.GetDefaultCredentials());

            var checkedProducts = string.Join(Environment.NewLine,
                dict.Select(kvp => $"{kvp.Key.Product.Name} (size - {kvp.Key.ProductSize?.Name ?? "NULL"}) amount = {kvp.Value}"));
            var restrictionsText = string.Join(Environment.NewLine,
                restrictions.ProductsExceedRemainingAmounts.Select(kvp =>
                    $"{kvp.Key.Product.Name} (size - {kvp.Key.ProductSize?.Name ?? "NULL"}) overdraft = {kvp.Value}"));

            MessageBox.Show($"Checked products:\n{checkedProducts}\n\n" +
                            $"Result: {restrictions.CheckResult}\n{restrictionsText}",
                "Selling restrictions", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Создать кухонный заказа. Заказ создается без официантской части.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private void CreateKitchenOrder()
        {
            var os = PluginContext.Operations;
            var readFormatInfo = new DateTimeFormatInfo { FullDateTimePattern = "yyyy-MM-dd'T'HH:mm:ss.fff", ShortDatePattern = "yyyy-MM-dd" };
            var printTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff"), readFormatInfo, DateTimeStyles.AssumeLocal);
            var orderNumber = 1;
            var cookingPlace = os.GetTerminalsGroupRestaurantSections(os.GetHostTerminalsGroup()).First();
            var cookingPlaceType = os.GetCookingPlaceTypes().First();
            var product1 = GetProduct();
            var product2 = GetProduct();
            var compound1 = GetProduct(true);
            var compound2 = GetProduct(true);
            var productSimpleModifier = os.GetSimpleModifiers(product1, null).FirstOrDefault();
            var productGroupModifier = os.GetGroupModifiers(product2, null).FirstOrDefault();
            var compoundItemTemplate = compound1.Template;
            var compoundSplittableGroupModifier = os.GetSplittableGroupModifiers(compoundItemTemplate, null).FirstOrDefault();
            var compoundCommonSimpleModifier = os.GetCommonSimpleModifiers(compoundItemTemplate, null).FirstOrDefault();
            KitchenOrderModifierItemDto productSimpleModifierDto = null;
            KitchenOrderModifierItemDto productGroupModifierDto = null;
            KitchenOrderModifierItemDto compoundSplittableGroupModifierDto = null;
            KitchenOrderModifierItemDto compoundCommonSimpleModifierDto = null;
            var compoundSize = compoundItemTemplate.Scale != null
                ? os.GetProductScaleSizes(compoundItemTemplate.Scale).First()
                : null;

            if (productSimpleModifier != null)
                productSimpleModifierDto = new KitchenOrderModifierItemDto
                {
                    BaseOrderItemId = Guid.NewGuid(),
                    CookingPlace = cookingPlace,
                    CookingPlaceType = cookingPlaceType,
                    CookingTime = TimeSpan.Parse("00:15:00"),
                    IsSeparate = false,
                    OrderRank = new Tuple<int, int>(0, 0),
                    AmountIndependentOfParentAmount = false,
                    IsHidden = false,
                    Product = productSimpleModifier.Product,
                    ItemSaleEventId = Guid.NewGuid(),
                    ItemCookingEventId = Guid.NewGuid(),
                    Amount = 1
                };
            if (productGroupModifier != null)
                productGroupModifierDto = new KitchenOrderModifierItemDto
                {
                    BaseOrderItemId = Guid.NewGuid(),
                    CookingPlace = cookingPlace,
                    CookingPlaceType = cookingPlaceType,
                    CookingTime = TimeSpan.Parse("00:15:00"),
                    IsSeparate = false,
                    OrderRank = new Tuple<int, int>(1, 0),
                    AmountIndependentOfParentAmount = false,
                    IsHidden = false,
                    Product = productGroupModifier.Items.First().Product,
                    ItemSaleEventId = Guid.NewGuid(),
                    ItemCookingEventId = Guid.NewGuid(),
                    Amount = 1
                };
            if (compoundSplittableGroupModifier != null)
                compoundSplittableGroupModifierDto = new KitchenOrderModifierItemDto
                {
                    BaseOrderItemId = Guid.NewGuid(),
                    CookingPlace = cookingPlace,
                    CookingPlaceType = cookingPlaceType,
                    CookingTime = TimeSpan.Parse("00:15:00"),
                    IsSeparate = false,
                    OrderRank = new Tuple<int, int>(2, 1),
                    AmountIndependentOfParentAmount = false,
                    IsHidden = false,
                    Product = compoundSplittableGroupModifier.Items.First().Product,
                    ItemSaleEventId = Guid.NewGuid(),
                    ItemCookingEventId = Guid.NewGuid(),
                    Amount = 1
                };
            if (compoundCommonSimpleModifier != null)
                compoundCommonSimpleModifierDto = new KitchenOrderModifierItemDto
                {
                    BaseOrderItemId = Guid.NewGuid(),
                    CookingPlace = cookingPlace,
                    CookingPlaceType = cookingPlaceType,
                    CookingTime = TimeSpan.Parse("00:15:00"),
                    IsSeparate = false,
                    OrderRank = new Tuple<int, int>(2, 0),
                    AmountIndependentOfParentAmount = false,
                    IsHidden = false,
                    Product = compoundCommonSimpleModifier.Product,
                    ItemSaleEventId = Guid.NewGuid(),
                    ItemCookingEventId = Guid.NewGuid(),
                    Amount = 1
                };

            var commentDto = new KitchenOrderItemCommentDto("Sample Plugin", false);
            var primaryCompoundItemComponent = new KitchenOrderCompoundItemComponentDto()
            {
                BaseCompoundItemComponentId = Guid.NewGuid(),
                ItemCookingEventId = Guid.NewGuid(),
                ItemSaleEventId = Guid.NewGuid(),
                Amount = 0.5m,
                Product = compound1,
                Modifiers = compoundSplittableGroupModifierDto == null
                        ? Array.Empty<KitchenOrderModifierItemDto>()
                        : new List<KitchenOrderModifierItemDto> { compoundSplittableGroupModifierDto }
            };
            var secondaryCompoundItemComponent = new KitchenOrderCompoundItemComponentDto
            {
                BaseCompoundItemComponentId = Guid.NewGuid(),
                ItemCookingEventId = Guid.NewGuid(),
                ItemSaleEventId = Guid.NewGuid(),
                Amount = 0.5m,
                Product = compound2,
                Modifiers = Array.Empty<KitchenOrderModifierItemDto>()
            };

            var kitchenOrders = os.GetKitchenOrders();
            if (kitchenOrders.Count > 0)
                orderNumber = kitchenOrders.Select(o => o.Number).Max() + 1;
            var order = new KitchenOrderDto
            {
                BaseOrderId = Guid.NewGuid(),
                ActualGuestsCount = 2,
                InitialGuestsCount = 2,
                Number = orderNumber,
                OrderOpenTime = DateTime.Now,
                Table = os.GetTables().First(),
                Waiter = os.GetUsers().Single(u => u.Name == "7 Админ"),
                OrderType = null,
            };
            IKitchenOrderCookingItemDto[] products = {
                    new KitchenOrderProductItemDto
                    {
                        BaseOrderItemId = Guid.NewGuid(),
                        ItemSaleEventId = Guid.NewGuid(),
                        Product = product1,
                        CookingPlace = cookingPlace,
                        CookingPlaceType = cookingPlaceType,
                        Amount = 1,
                        CookingTime = TimeSpan.Parse("00:15:00"),
                        PrintTime = printTime,
                        Comment = commentDto,
                        GuestName = "Гость 1",
                        OrderRank = 0,
                        ProductSize = null,
                        ServeGroupNumber = 1,
                        Modifiers = productSimpleModifierDto == null
                            ? Array.Empty<KitchenOrderModifierItemDto>()
                            : new List<KitchenOrderModifierItemDto>{ productSimpleModifierDto }
                    },
                    new KitchenOrderProductItemDto
                    {
                        BaseOrderItemId = Guid.NewGuid(),
                        ItemSaleEventId = Guid.NewGuid(),
                        Product = product2,
                        CookingPlace = cookingPlace,
                        CookingPlaceType = cookingPlaceType,
                        Amount = 1,
                        CookingTime = TimeSpan.Parse("00:15:00"),
                        PrintTime = printTime,
                        Comment = null,
                        GuestName = "Гость 1",
                        OrderRank = 1,
                        ProductSize = null,
                        ServeGroupNumber = 1,
                        Modifiers = productGroupModifierDto == null
                            ? Array.Empty<KitchenOrderModifierItemDto>()
                            : new List<KitchenOrderModifierItemDto>{ productGroupModifierDto }
                    },
                    new KitchenOrderCompoundItemDto
                    {
                        BaseOrderItemId = Guid.NewGuid(),
                        CookingPlace = cookingPlace,
                        CookingPlaceType = cookingPlaceType,
                        CookingTime = TimeSpan.Parse("00:15:00"),
                        PrintTime = printTime,
                        Comment = null,
                        GuestName = "Гость 2",
                        OrderRank = 2,
                        ProductSize = compoundSize,
                        ServeGroupNumber = 1,
                        CompoundItemTemplate = compoundItemTemplate,
                        PrimaryComponent = primaryCompoundItemComponent,
                        SecondaryComponent = secondaryCompoundItemComponent,
                        CommonModifiers = compoundCommonSimpleModifierDto == null
                            ? Array.Empty<KitchenOrderModifierItemDto>()
                            : new List<KitchenOrderModifierItemDto>{ compoundCommonSimpleModifierDto }
                    }
            };

            order.Items = products;
            os.CreateKitchenOrder(order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Создать пустой кухонный заказа. Заказ создается без официантской части.
        /// </summary>
        private void CreateEmptyKitchenOrder()
        {
            var os = PluginContext.Operations;
            var number = 1;
            var kitchenOrders = os.GetKitchenOrders();
            if (kitchenOrders.Count > 0)
                number = kitchenOrders.Select(o => o.Number).Max() + 1;
            var order = new KitchenOrderDto
            {
                BaseOrderId = Guid.NewGuid(),
                ActualGuestsCount = 1,
                InitialGuestsCount = 1,
                Number = number,
                OrderOpenTime = DateTime.Now,
                Table = os.GetTables().First(),
                Waiter = os.GetUsers().Single(u => u.Name == "7 Админ"),
                OrderType = null,
                Items = Array.Empty<IKitchenOrderCookingItemDto>()
            };
            os.CreateKitchenOrder(order, os.GetDefaultCredentials());
        }

        /// <summary>
        /// Удалить все блюда из кухонного заказа, созданного из АПИ.
        /// </summary>
        private void DeleteKitchenOrderItems()
        {
            var kitchenOrder = PluginContext.Operations.GetKitchenOrders().Last(o => o.IsExternal);
            PluginContext.Operations.DeleteKitchenOrderItems(kitchenOrder);
        }

        private void Print()
        {
            var printingDevices = PluginContext.Operations.GetPrintingDeviceInfos();
            if (printingDevices.Count <= 0)
                return;

            var result = PluginContext.Operations.Print(printingDevices.First(), new Document
            {
                Markup = new XElement(Tags.Doc, new XElement(Tags.Center, "Test Printer"))
            });
            MessageBox.Show(printingDevices.First().FriendlyName + " " + result);
        }

        private void ShowCustomDataDialogWindow()
        {
            var customDataWindow = new CustomData.CustomDataWindow
            {
                Owner = window
            };
            customDataWindow.ShowDialog();
        }

        private void IsConnectedToMainTerminal()
        {
            MessageBox.Show(PluginContext.Operations.IsConnectedToMainTerminal()
                    ? "Connected"
                    : "Disconnected");
        }

        private void GetUserFullName()
        {
            var fullName = PluginContext.Operations.GetUserFullName(PluginContext.Operations.GetUsers().First());
        }

        private void GetAddressSettings()
        {
            var restaurant = PluginContext.Operations.GetHostRestaurant();

            MessageBox.Show($@"UseNewFormat:{restaurant.AddressShowTypeSettings.UseNewFormat}" + Environment.NewLine +
                            $@"UseLiveSearch:{restaurant.AddressShowTypeSettings.UseLiveSearch}" + Environment.NewLine +
                            $@"AddressShowType:{restaurant.AddressShowTypeSettings.AddressShowType}",
                            "Address Settings");
        }

        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
        }
    }
}
