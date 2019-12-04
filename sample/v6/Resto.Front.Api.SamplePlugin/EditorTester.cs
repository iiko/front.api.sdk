using System;
using System.Collections.Generic;
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

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class EditorTester : IDisposable
    {
        private Window window;
        private ItemsControl buttons;
        private const string PluginName = "Resto.Front.Api.SamplePlugin";
        private readonly Random rnd = new Random();

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

            window.ShowDialog();
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

        [NotNull]
        private static ISubmittedEntities SubmitChanges([NotNull] IEditSession editSession)
        {
            var credentials = PluginContext.Operations.GetCredentials();
            return PluginContext.Operations.SubmitChanges(credentials, editSession);
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
            var result = SubmitChanges(editSession);

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
            var newOrder = editSession.CreateOrder(null, selectedUser); // заказ будет создан на столе по умолчанию
            editSession.ChangeOrderOriginName("Sample Plugin", newOrder);
            editSession.AddOrderGuest(null, newOrder);
            var guest2 = editSession.AddOrderGuest("Alex", newOrder);
            var result = SubmitChanges(editSession);

            var previouslyCreatedOrder = result.Get(newOrder);
            var previouslyAddedGuest = result.Get(guest2);

            PluginContext.Operations.AddOrderProductItem(17.3m, product, previouslyCreatedOrder, previouslyAddedGuest, null, credentials);
        }

        /// <summary>
        /// Добавление гостя John Doe. 
        /// </summary>
        private void AddGuest()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.AddOrderGuest("John Doe", order, credentials);
        }

        /// <summary>
        /// Привязать гостя Semen с номером карты "0123456789" к заказу.
        /// </summary>
        private void AddClientToOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var discountType = PluginContext.Operations.GetDiscountTypes().LastOrDefault(d => d.CanApplyByDiscountCard);
            const string cardNumber = "0123456789";
            const string clientName = "Semen";

            var client = PluginContext.Operations.CreateClient(Guid.NewGuid(), clientName, null, cardNumber, DateTime.Now, credentials);

            var card = PluginContext.Operations.SearchDiscountCardByNumber(cardNumber);
            if (card == null)
                PluginContext.Operations.CreateDiscountCard(cardNumber, clientName, null, discountType);
            else
                PluginContext.Operations.UpdateDiscountCard(card.Id, cardNumber, clientName, null, discountType);

            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            PluginContext.Operations.AddClientToOrder(credentials, order, client);
        }

        /// <summary>
        /// Отвязать гостя Semen от заказа.
        /// </summary>
        private void RemoveOrderClient()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var client = PluginContext.Operations.SearchClients("Semen").Last();
            PluginContext.Operations.RemoveOrderClient(credentials, order, client);
        }

        /// <summary>
        /// Добавление продукта из номенклатуры.
        /// </summary>
        private void AddProduct()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var product = GetProduct();
            var order = PluginContext.Operations.GetOrders().Last();
            var guest = order.Guests.Last();
            PluginContext.Operations.AddOrderProductItem(42m, product, order, guest, null, credentials);
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
        private void AddCompoundItem()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var guest = order.Guests.Last();
            var product = GetProduct(true);
            var template = product.Template;
            // ReSharper disable once PossibleNullReferenceException
            var size = template.Scale != null ? PluginContext.Operations.GetProductScaleSizes(template.Scale).First() : null;
            var editSession = PluginContext.Operations.CreateEditSession();
            var compoundItem = editSession.AddOrderCompoundItem(product, order, guest, size);
            var primaryComponent = editSession.AddPrimaryComponent(product, order, compoundItem);
            editSession.ChangeOrderCookingItemAmount(42m, compoundItem, order);
            var modifierDefaultAmounts = PluginContext.Operations.GetTemplatedModifiersParamsByProduct(product);
            AddDefaultCompoundItemModifiers(compoundItem, template, modifierDefaultAmounts, order, editSession);
            AddDefaultCompoundItemComponentModifiers(primaryComponent, template, modifierDefaultAmounts, order, editSession);

            SubmitChanges(editSession);
        }

        /// <summary>
        /// Добавление составного блюда.
        /// </summary>
        private void AddSplittedCompoundItem()
        {
            var editSession = PluginContext.Operations.CreateEditSession();
            AddSplittedCompoundItemInternal(editSession);

            SubmitChanges(editSession);
        }

        private INewOrderCompoundItemStub AddSplittedCompoundItemInternal([NotNull] IEditSession editSession)
        {
            var order = PluginContext.Operations.GetOrders().Last();
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

        private static void AddDefaultCompoundItemModifiers(IOrderCompoundItemStub compoundItem, ICompoundItemTemplate template, IReadOnlyList<ITemplatedModifierParams> modifierParams, IOrderStub order, IEditSession editSession)
        {
            foreach (var commonGroupModifier in template.GetCommonGroupModifiers(PluginContext.Operations))
            {
                foreach (var commonChildModifier in commonGroupModifier.Items)
                {
                    var defaultAmount = GetDefaultModifierAmount(modifierParams, commonGroupModifier.ProductGroup, commonChildModifier.Product, commonChildModifier.DefaultAmount);
                    if (defaultAmount > 0)
                        editSession.AddOrderModifierItem(defaultAmount, commonChildModifier.Product, commonGroupModifier.ProductGroup, order, compoundItem);
                }
            }

            foreach (var commonSimpleModifier in template.GetCommonSimpleModifiers(PluginContext.Operations))
            {
                var defaultAmount = GetDefaultModifierAmount(modifierParams, null, commonSimpleModifier.Product, commonSimpleModifier.DefaultAmount);
                if (defaultAmount > 0)
                    editSession.AddOrderModifierItem(defaultAmount, commonSimpleModifier.Product, null, order, compoundItem);
            }
        }

        private static void AddDefaultCompoundItemComponentModifiers(IOrderCompoundItemComponentStub component, ICompoundItemTemplate template, IReadOnlyList<ITemplatedModifierParams> modifierParams, IOrderStub order, IEditSession editSession)
        {
            foreach (var splittableGroupModifier in template.GetSplittableGroupModifiers(PluginContext.Operations))
            {
                foreach (var splittableChildModifier in splittableGroupModifier.Items)
                {
                    var defaultAmount = GetDefaultModifierAmount(modifierParams, splittableGroupModifier.ProductGroup, splittableChildModifier.Product, splittableChildModifier.DefaultAmount);
                    if (defaultAmount > 0)
                        editSession.AddOrderModifierItem(defaultAmount, splittableChildModifier.Product, splittableGroupModifier.ProductGroup, order, component);
                }
            }

            foreach (var splittableSimpleModifier in template.GetSplittableSimpleModifiers(PluginContext.Operations))
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
        private void AddModifier()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var modifier = orderItem.AvailableSimpleModifiers.First();
            PluginContext.Operations.AddOrderModifierItem(1, modifier.Product, null, order, orderItem, credentials);
        }

        /// <summary>
        /// Добавление группового модификатора.
        /// </summary>
        private void AddGroupModifier()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var groupModifier = orderItem.AvailableGroupModifiers.Last();
            PluginContext.Operations.AddOrderModifierItem(1, groupModifier.Items.Last().Product, groupModifier.ProductGroup, order, orderItem, credentials);
        }

        /// <summary>
        /// Увеличение количества последнего блюда заказа на 1.
        /// </summary>
        private void IncreaseAmount()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            PluginContext.Operations.ChangeOrderCookingItemAmount(orderItem.Amount + 1, orderItem, order, credentials);
        }

        /// <summary>
        /// Уменьшение количества последнего блюда заказа на 1.
        /// </summary>
        private void DecreaseAmount()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            PluginContext.Operations.ChangeOrderCookingItemAmount(orderItem.Amount - 1, orderItem, order, credentials);
        }

        /// <summary>
        /// Задание количества последнего блюда заказа.
        /// </summary>
        private void SetAmount()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            var amount = 1.1m;
            PluginContext.Operations.ChangeOrderCookingItemAmount(amount, orderItem, order, credentials);
        }

        /// <summary>
        /// Создание нового гостя и перенос ему последнего блюда заказа.
        /// </summary>
        private void MoveProductToNewGuest()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.Last();
            var editSession = PluginContext.Operations.CreateEditSession();
            var guest = editSession.AddOrderGuest("New guest", order);
            editSession.MoveOrderItemToAnotherGuest(orderItem, guest, order);

            SubmitChanges(editSession);
        }

        private void MoveToAnotherWaiter()
        {
            if (!ChooseItemDialogHelper.ShowDialog(PluginContext.Operations.GetUsers(), user => user.Name, out var selectedUser, "Select new waiter", window))
                return;

            var order = PluginContext.Operations.GetOrders().Last();
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.ChangeOrderWaiter(order, selectedUser);

            SubmitChanges(editSession);
        }

        /// <summary>
        /// Разделение последнего блюда на две части и назначение получившегося блюда на другого гостя (если есть)
        /// </summary>
        private void SplitProduct()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            var amountToSplit = 0.25m;
            var editSession = PluginContext.Operations.CreateEditSession();
            var item = editSession.SplitOrderCookingItem(amountToSplit, order, orderItem);
            var guest = order.Guests.FirstOrDefault(p => p != orderItem.Guest);
            if (guest != null)
                editSession.MoveOrderItemToAnotherGuest(item, guest, order);

            SubmitChanges(editSession);
        }

        /// <summary>
        /// Сервисная печать последнего блюда заказа.
        /// </summary>
        private void PrintProduct()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var itemsToPrint = new[] { order.Items.OfType<IOrderCookingItem>().Last() };
            PluginContext.Operations.PrintOrderItems(credentials, order, itemsToPrint);
        }

        /// <summary>
        /// Сервисная печать заказа.
        /// </summary>
        private void PrintOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var itemsToPrint = order.Items.OfType<IOrderCookingItem>().ToList();
            PluginContext.Operations.PrintOrderItems(credentials, order, itemsToPrint);
        }

        /// <summary>
        /// Печать пречека.
        /// </summary>
        private void PrintBillCheque()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.PrintBillCheque(credentials, order);
        }

        /// <summary>
        /// Печать пречека.
        /// </summary>
        private void BillOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.BillOrder(credentials, order, 32);
        }

        /// <summary>
        /// Отмена пречека.
        /// </summary>
        private void CancelBill()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.CancelBill(credentials, order);
        }

        /// <summary>
        /// Удаление последнего гостя заказа.
        /// </summary>
        private void DeleteGuest()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var guest = order.Guests.Last();
            PluginContext.Operations.DeleteOrderGuest(order, guest, credentials);
        }

        /// <summary>
        /// Удаление последнего блюда из заказа.
        /// </summary>
        private void DeleteProduct()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.Last();
            PluginContext.Operations.DeleteOrderItem(order, orderItem, credentials);
        }

        /// <summary>
        /// Удаление отпечатанного блюда из заказа.
        /// </summary>
        private void DeletePrintedProduct()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(x => x.Status == OrderStatus.New);
            var orderItem = order.Items.Last();
            var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last();
            const string comment = "Причина списания продукта";

            if (removalType.WriteoffType.Equals(WriteoffType.None))
            {
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, orderItem, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Cafe))
            {
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WriteoffToCafe(removalType), order, orderItem, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Waiter))
            {
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WriteoffToWaiter(removalType), order, orderItem, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.User))
            {
                var user = PluginContext.Operations.GetUsers().First(u => u.IsSessionOpen);
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WriteoffToUser(removalType, user), order, orderItem, credentials);
                return;
            }
            throw new NotSupportedException($"Write-off type '{removalType.WriteoffType}' is not supported.");
        }

        /// <summary>
        /// Удаление модификатора из заказа.
        /// </summary>
        private void DeleteModifier()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var orderItemModifier = orderItem.AssignedModifiers.Last();
            PluginContext.Operations.DeleteOrderModifierItem(order, orderItem, orderItemModifier, credentials);
        }

        /// <summary>
        /// Удаление отпечатанного модификатора из заказа.
        /// </summary>
        private void DeletePrintedModifier()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var orderItemModifier = orderItem.AssignedModifiers.Last();
            var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last();
            const string comment = "Причина списания модификатора";

            if (removalType.WriteoffType.Equals(WriteoffType.None))
            {
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, orderItem, orderItemModifier, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Cafe))
            {
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToCafe(removalType), order, orderItem, orderItemModifier, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Waiter))
            {
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToWaiter(removalType), order, orderItem, orderItemModifier, credentials);
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.User))
            {
                var user = PluginContext.Operations.GetUsers().First(u => u.IsSessionOpen);
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToUser(removalType, user), order, orderItem, orderItemModifier, credentials);
                return;
            }
            throw new NotSupportedException($"Write-off type '{removalType.WriteoffType}' is not supported.");
        }

        /// <summary>
        /// Добавление комментария к блюду.
        /// </summary>
        private void AddProductComment()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.Status == OrderItemStatus.Added);
            PluginContext.Operations.ChangeOrderItemComment("Приготовить без соли.", order, orderItem, credentials);
        }

        /// <summary>
        /// Удаление комментария к блюду.
        /// </summary>
        private void DeleteProductComment()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.Status == OrderItemStatus.Added);
            PluginContext.Operations.DeleteOrderItemComment(order, orderItem, credentials);
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

        private void CreateDelivery(bool withOriginName)
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
            var frontDeliverySettings = PluginContext.Operations.GetTerminalDeliveryDuration();
            var duration = frontDeliverySettings.CourierDeliveryTime ?? 60;
            var expectedDeliverTime = now.AddMinutes(duration);
            var deliveryOrder = editSession.CreateDeliveryOrder(1149, now, primaryPhone.PhoneValue, address, expectedDeliverTime, orderType, client, deliveryOperator, TimeSpan.FromMinutes(duration));
            if (withOriginName)
                editSession.ChangeOrderOriginName(DeliveryOriginName, deliveryOrder);
            editSession.AddOrderGuest("Alex", deliveryOrder);
            editSession.ChangeClientSurname("Petrov", client);
            editSession.ChangeClientNick("Batman", client);
            editSession.ChangeClientEmails(new List<EmailDto> { primaryEmail, secondaryEmail }, client);
            editSession.ChangeClientComment("Customer's Comment", client);
            editSession.ChangeClientAddresses(new List<AddressDto> { address }, 0, client);
            editSession.ChangeClientCardNumber("123456798", client);
            editSession.ChangeDeliveryComment("Delivery comment", deliveryOrder);
            editSession.ChangeDeliveryEmail(primaryEmail.EmailValue, deliveryOrder);

            SubmitChanges(editSession);
        }

        /// <summary>
        /// Показать доставки с непустым ключом источника
        /// </summary>
        private void ShowDeliveriesWithNotEmptyOriginName()
        {
            var deliveriesWithKey = PluginContext.Operations.GetDeliveryOrders()
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
            var createdDeliveryByPlugin = PluginContext.Operations.GetDeliveryOrders().LastOrDefault(d => d.DeliveryStatus == DeliveryStatus.New && d.OrderType.OrderServiceType == OrderServiceTypes.DeliveryByCourier);
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

            SubmitChanges(editSession);
        }

        /// <summary>
        /// Изменение типа созданной доставки с самовывозной на курьерскую.
        /// </summary>
        private void ChangeDeliveryOrderTypeOnCourier()
        {
            var createdDeliveryByPlugin = PluginContext.Operations.GetDeliveryOrders().LastOrDefault(d => d.DeliveryStatus == DeliveryStatus.New && d.OrderType.OrderServiceType == OrderServiceTypes.DeliveryByClient);
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
            SubmitChanges(editSession);
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

            SubmitChanges(editSession);
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
        private void AddDiscount()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var discountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && !x.DiscountByFlexibleSum);
            PluginContext.Operations.AddDiscount(discountType, order, credentials);
        }

        /// <summary>
        /// Добавление скидки на произвольную сумму.
        /// </summary>
        private void AddFlexibleSumDiscount()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var discountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.DiscountByFlexibleSum);
            PluginContext.Operations.AddFlexibleSumDiscount(50, discountType, order, credentials);
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

            var updatedCard = PluginContext.Operations.UpdateDiscountCard(existingCard.Id, cardNumber, "Mike", priceCategory, null);
            PluginContext.Log.Info($"Updated card owner from {existingCard.OwnerName} to {updatedCard.OwnerName}.");
        }

        /// <summary>
        /// Добавление скидки и связанного с ним гостя.
        /// </summary>
        private void AddDiscountByCardNumber()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var discountCard = PluginContext.Operations.GetDiscountCards().Last(x => x.DiscountType != null);
            PluginContext.Operations.AddDiscountByCardNumber(discountCard.CardNumber, order, discountCard, credentials);
        }

        /// <summary>
        /// Удаление скидки и связанного с ним гостя.
        /// </summary>
        private void DeleteDiscount()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var cardDiscountItem = order.Discounts.Last(discount => discount.DiscountType.CanApplyByDiscountCard);
            PluginContext.Operations.DeleteDiscount(cardDiscountItem, order, credentials);
        }

        /// <summary>
        /// Добавление скидки с возможностью выбора блюд.
        /// </summary>
        private void AddSelectiveDiscount()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var selectedDish = new List<IOrderProductItemStub> { order.Items.OfType<IOrderProductItem>().Last() };
            var discountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.CanApplySelectively);

            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.AddDiscount(discountType, order);
            editSession.ChangeSelectiveDiscount(order, discountType, selectedDish, null, null);
            SubmitChanges(editSession);
        }

        /// <summary>
        /// Добавление комбо в заказ.
        /// </summary>
        private void AddComboInOrder()
        {
            var editSession = PluginContext.Operations.CreateEditSession();

            var order = PluginContext.Operations.GetOrders().Last();
            var guest = order.Guests.Last();
            var compoundItem = AddSplittedCompoundItemInternal(editSession);
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
            SubmitChanges(editSession);
        }

        /// <summary>
        /// Добавление дополнительной информации в заказ.
        /// </summary>
        private static void AddExternalDataToOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.AddOrderExternalData(PluginName, "Sample plugin external data", order, credentials);

            var value = PluginContext.Operations.TryGetOrderExternalDataByKey(order.Id, PluginName);
            MessageBox.Show($"Sample plugin external data value: {value}");
        }

        /// <summary>
        /// Удаление дополнительной информации из заказа.
        /// </summary>
        private static void DeleteExternalDataFromOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.DeleteOrderExternalData(PluginName, order, credentials);
        }

        /// <summary>
        /// Отменить новую доставку.
        /// </summary>
        private void CancelNewDelivery()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var cancelCause = PluginContext.Operations.GetDeliveryCancelCauses().First();
            PluginContext.Operations.CancelNewDelivery(credentials, order, cancelCause);
        }

        /// <summary>
        /// Удалить заказ и не сохранить его элементы в OLAP как удаленные.
        /// </summary>
        private void DeleteOrderAndHideItemsFromOlap()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(o => !(o is IDeliveryOrder) && o.Status == OrderStatus.New);
            var printedItems = order.Items.Where(i => i.PrintTime.HasValue && !i.Deleted).ToList();

            if (printedItems.Count > 0)
            {
                var editSession = PluginContext.Operations.CreateEditSession();
                var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last(rt => rt.WriteoffType.Equals(WriteoffType.None));
                const string comment = "Test API DeleteOrderAndHideItemsFromOlap";

                foreach (var item in printedItems)
                    editSession.DeletePrintedOrderItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, item);

                SubmitChanges(editSession);
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
                    editSession.DeletePrintedOrderItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, item);

                SubmitChanges(editSession);
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
                    MessageBox.Show("Document successfully printed", string.Empty, MessageBoxButton.OK, MessageBoxImage.None);
                else
                    MessageBox.Show("Print error", string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);

                printerSelectionWindow.Close();
            });

            var printers = new Dictionary<string, IPrinterRef>
            {
                { "Bill printer", PluginContext.Operations.TryGetBillPrinter(checkIsConfigured: true) },
                { "Document printer", PluginContext.Operations.TryGetDocumentPrinter(checkIsConfigured: true) },
                { "Sticker printer", PluginContext.Operations.TryGetStickerPrinter(checkIsConfigured: true) },
                { "Receipt printer", PluginContext.Operations.TryGetReceiptChequePrinter() },
                { "Report printer", PluginContext.Operations.GetReportPrinter() }
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
            SubmitChanges(editSession);
        }

        private void AddProductAndSetOpenPrice()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var openPriceDish = PluginContext.Operations.GetActiveProducts().Last(i => i.Type != ProductType.ForPurchase && i.CanSetOpenPrice);

            var editSession = PluginContext.Operations.CreateEditSession();
            var guest = editSession.AddOrderGuest("Alex", order);
            var product = editSession.AddOrderProductItem(1, openPriceDish, order, guest, null);
            editSession.SetOpenPrice(1000m, product, order);

            SubmitChanges(editSession);
        }

        private void ChangePriceCategory()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(i => i.Status == OrderStatus.New);
            var priceCategory = PluginContext.Operations.GetPriceCategories().Last(i => i.CanApplyManually && !i.Deleted);
            PluginContext.Operations.ChangePriceCategory(priceCategory, order, credentials);
        }

        private void ResetPriceCategory()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(i => i.Status == OrderStatus.New);
            PluginContext.Operations.ResetPriceCategory(order, credentials);
        }

        private static void StartService()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(x => x.Status == OrderStatus.New);
            var serviceProduct = PluginContext.Operations.GetActiveProducts().Last(x => x.Type == ProductType.Service && x.RateSchedule != null);
            var service = PluginContext.Operations.AddOrderServiceItem(serviceProduct, order, order.Guests.Last(), credentials, TimeSpan.FromHours(2));
            PluginContext.Operations.StartService(credentials, order, service);
        }

        private static void StopService()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var order = PluginContext.Operations.GetOrders().Last(x => x.Status == OrderStatus.New);
            var service = order.Items.OfType<IOrderServiceItem>().Last(x => x.IsStarted);
            PluginContext.Operations.StopService(credentials, order, service);
        }

        private static void ChangeOrderTables()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var tables = PluginContext.Operations.GetRestaurantSections().First().Tables;
            var order = PluginContext.Operations.GetOrders().Last(x => x.Status == OrderStatus.New);
            PluginContext.Operations.ChangeOrderTables(order, new[] { tables.First() }, credentials);
        }

        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
        }
    }
}
