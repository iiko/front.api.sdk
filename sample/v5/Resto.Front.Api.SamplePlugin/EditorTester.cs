using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Resto.Front.Api.V5.Data.Brd;
using Resto.Front.Api.V5.Data.DataTransferObjects;
using Resto.Front.Api.V5.Data.Orders;
using Resto.Front.Api.V5.Data.Organization;
using Resto.Front.Api.V5.Editors;
using Resto.Front.Api.V5.Extensions;
using Resto.Front.Api.V5;
using System.Windows.Controls;
using System.Threading;
using Resto.Front.Api.V5.Attributes.JetBrains;
using Resto.Front.Api.V5.Data.Assortment;
using Resto.Front.Api.V5.Editors.Stubs;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class EditorTester : IDisposable
    {
        private Window window;
        private ListBox buttons;
        private const string PluginName = "Resto.Front.Api.SamplePlugin";

        public EditorTester()
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
                Width = 200,
                Height = 500,
                Topmost = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                ResizeMode = ResizeMode.CanMinimize,
                ShowInTaskbar = true,
                VerticalContentAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.SingleBorderWindow,
            };
            AddButton("Create order", CreateOrder);
            AddButton("Add guest", AddGuest);
            AddButton("Add product", AddProduct);
            AddButton("Add modifier", AddModifier);
            AddButton("Add group modifier", AddGroupModifier);
            AddButton("Increase amount", IncreaseAmount);
            AddButton("Decrease amount", DecreaseAmount);
            AddButton("Set amount", SetAmount);
            AddButton("Move product", MoveProductToNewGuest);
            AddButton("Print product", PrintProduct);
            AddButton("Print order", PrintOrder);
            AddButton("Print bill cheque", PrintBillCheque);
            AddButton("Delete guest", DeleteGuest);
            AddButton("Delete product", DeleteProduct);
            AddButton("Delete printed product", DeletePrintedProduct);
            AddButton("Delete modifier", DeleteModifier);
            AddButton("Delete printed modifier", DeletePrintedModifier);
            AddButton("Add product comment", AddProductComment);
            AddButton("Delete product comment", DeleteProductComment);
            AddButton("Create delivery", CreateDelivery);
            AddButton("Create delivery with source key", CreateDeliveryWithOriginName);
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
            AddButton("Add external data in order", AddExternalDataInOrder);

            window.ShowDialog();
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
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", text, Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            buttons.Items.Add(button);
        }

        [NotNull]
        ISubmittedEntities SubmitChanges([NotNull] IEditSession editSession)
        {
            return PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
        }

        /// <summary>
        /// Создание заказа с добавлением  гостя Alex и продукта номенклатуры.
        /// </summary>   
        private void CreateOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();

            var editSession = PluginContext.Operations.CreateEditSession();
            var newOrder = editSession.CreateOrder(null);
            editSession.ChangeOrderOriginName("Sample Plugin", newOrder);
            var guest1 = editSession.AddOrderGuest(null, newOrder);
            var guest2 = editSession.AddOrderGuest("Alex", newOrder);
            var result = PluginContext.Operations.SubmitChanges(credentials, editSession);


            var previouslyCreatedOrder = result.Get(newOrder);
            var previouslyAddedGuest = result.Get(guest2);

            PluginContext.Operations.AddOrderProductItem(17.3m, PluginContext.Operations.GetActiveProducts().Last(),
                previouslyCreatedOrder, previouslyAddedGuest, null, credentials);
        }

        /// <summary>
        /// Добавление гостя John Doe. 
        /// </summary>
        private void AddGuest()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.AddOrderGuest("John Doe", order, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Привязать гостя Semen с номером карты "0123456789" к заказу.
        /// </summary>
        private void AddClientToOrder()
        {
            var editSession = PluginContext.Operations.CreateEditSession();
            var now = DateTime.Now;
            var cardDiscountType = PluginContext.Operations.GetDiscountTypes().LastOrDefault(d => d.CanApplyByDiscountCard);
            editSession.CreateClient(Guid.NewGuid(), "Semen", null, "0123456789", now, null, cardDiscountType);
            SubmitChanges(editSession);

            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var client = PluginContext.Operations.SearchClients("Semen").Last();
            PluginContext.Operations.AddClientToOrder(PluginContext.Operations.GetCredentials(), order, client);
        }

        /// <summary>
        /// Отвязать гостя Semen от заказа.
        /// </summary>
        private void RemoveOrderClient()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var client = PluginContext.Operations.SearchClients("Semen").Last();
            PluginContext.Operations.RemoveOrderClient(PluginContext.Operations.GetCredentials(), order, client);
        }

        /// <summary>
        /// Добавление продукта из номенклатуры.
        /// </summary>
        private void AddProduct()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var guest = order.Guests.Last();
            var product = PluginContext.Operations.GetActiveProducts().Last();
            PluginContext.Operations.AddOrderProductItem(42m, product, order, guest, null, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Добавление составного блюда.
        /// </summary>
        private void AddCompoundItem()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var guest = order.Guests.Last();
            var product = PluginContext.Operations.GetActiveProducts().Last(p => p.Template != null);
            var template = product.Template;
            var size = template.Scale != null ? PluginContext.Operations.GetProductScaleSizes(template.Scale).First() : null;
            var editSession = PluginContext.Operations.CreateEditSession();
            var compoundItem = editSession.AddOrderCompoundItem(product, order, guest, size);
            var primaryComponent = editSession.AddPrimaryComponent(product, order, compoundItem);
            editSession.ChangeOrderCookingItemAmount(42m, compoundItem, order);
            var modifierDefaultAmounts = PluginContext.Operations.GetTemplatedModifiersParamsByProduct(product);
            AddDefaultCompoundItemModifiers(compoundItem, template, modifierDefaultAmounts, order, editSession);
            AddDefaultCompoundItemComponentModifiers(primaryComponent, template, modifierDefaultAmounts, order, editSession);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
        }

        /// <summary>
        /// Добавление составного блюда.
        /// </summary>
        private void AddSplittedCompoundItem()
        {
            var editSession = PluginContext.Operations.CreateEditSession();
            AddSplittedCompoundItemInternal(editSession);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
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
            var compoundItem = editSession.AddOrderCompoundItem(templateProducts.First(), order, guest, size: size);
            var primaryComponent = editSession.AddPrimaryComponent(templateProducts.First(), order, compoundItem);
            var secondaryComponent = editSession.AddSecondaryComponent(templateProducts.Skip(1).First(), order, compoundItem);
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
            return specificAmount != null
                ? specificAmount.DefaultAmount
                : generalDefaultAmount;
        }

        /// <summary>
        /// Добавление одиночного модификатора.
        /// </summary>
        private void AddModifier()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var modifier = orderItem.AvailableSimpleModifiers.First();
            PluginContext.Operations.AddOrderModifierItem(1, modifier.Product, null, order, orderItem,
                PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Добавление группового модификатора.
        /// </summary>
        private void AddGroupModifier()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var groupModifier = orderItem.AvailableGroupModifiers.Last();
            PluginContext.Operations.AddOrderModifierItem(1, groupModifier.Items.Last().Product,
                groupModifier.ProductGroup, order, orderItem, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Увеличение количества последнего блюда заказа на 1.
        /// </summary>
        private void IncreaseAmount()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            PluginContext.Operations.ChangeOrderCookingItemAmount(orderItem.Amount + 1, orderItem, order, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Уменьшение количества последнего блюда заказа на 1.
        /// </summary>
        private void DecreaseAmount()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            PluginContext.Operations.ChangeOrderCookingItemAmount(orderItem.Amount - 1, orderItem, order, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Задание количества последнего блюда заказа.
        /// </summary>
        private void SetAmount()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last();
            var amount = 1.1m;
            PluginContext.Operations.ChangeOrderCookingItemAmount(amount, orderItem, order, PluginContext.Operations.GetCredentials());
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

        /// <summary>
        /// Сервисная печать последнего блюда заказа.
        /// </summary>
        private void PrintProduct()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.PrintOrderItems(PluginContext.Operations.GetCredentials(), order,
                new List<IOrderRootItem> { order.Items.Last() });
        }

        /// <summary>
        /// Сервисная печать заказа.
        /// </summary>
        private void PrintOrder()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.PrintOrderItems(PluginContext.Operations.GetCredentials(), order,
                order.Items);
        }

        /// <summary>
        /// Печать пречека.
        /// </summary>
        private void PrintBillCheque()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.PrintBillCheque(PluginContext.Operations.GetCredentials(), order);
        }

        /// <summary>
        /// Удаление последнего гостя заказа.
        /// </summary>
        private void DeleteGuest()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var guest = order.Guests.Last();
            PluginContext.Operations.DeleteOrderGuest(order, guest, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Удаление последнего блюда из заказа.
        /// </summary>
        private void DeleteProduct()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.Last();
            PluginContext.Operations.DeleteOrderItem(order, orderItem, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Удаление отпечатанного блюда из заказа.
        /// </summary>
        private void DeletePrintedProduct()
        {
            var order = PluginContext.Operations.GetOrders().Last(x => x.Status == OrderStatus.New);
            var orderItem = order.Items.Last();
            var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last();
            const string comment = "Причина списания продукта";

            if (removalType.WriteoffType.Equals(WriteoffType.None))
            {
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, orderItem, PluginContext.Operations.GetCredentials());
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Cafe))
            {
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WriteoffToCafe(removalType), order, orderItem, PluginContext.Operations.GetCredentials());
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Waiter))
            {
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WriteoffToWaiter(removalType), order, orderItem, PluginContext.Operations.GetCredentials());
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.User))
            {
                PluginContext.Operations.DeletePrintedOrderItem(comment, WriteoffOptions.WriteoffToUser(removalType, PluginContext.Operations.GetUsers().First(u => u.IsSessionOpen)), order,
                    orderItem, PluginContext.Operations.GetCredentials());
                return;
            }
            throw new NotSupportedException(string.Format("Write-off type '{0}' is not supported.", removalType.WriteoffType));
        }

        /// <summary>
        /// Удаление модификатора из заказа.
        /// </summary>
        private void DeleteModifier()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var orderItemModifier = orderItem.AssignedModifiers.Last();
            PluginContext.Operations.DeleteOrderModifierItem(order, orderItem, orderItemModifier,
                PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Удаление отпечатанного модификатора из заказа.
        /// </summary>
        private void DeletePrintedModifier()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderProductItem>().Last();
            var orderItemModifier = orderItem.AssignedModifiers.Last();
            var removalType = PluginContext.Operations.GetActiveRemovalTypes().Last();
            const string comment = "Причина списания модификатора";

            if (removalType.WriteoffType.Equals(WriteoffType.None))
            {
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, orderItem, orderItemModifier,
                    PluginContext.Operations.GetCredentials());
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Cafe))
            {
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToCafe(removalType), order, orderItem, orderItemModifier,
                    PluginContext.Operations.GetCredentials());
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.Waiter))
            {
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToWaiter(removalType), order, orderItem, orderItemModifier,
                    PluginContext.Operations.GetCredentials());
                return;
            }
            if (removalType.WriteoffType.HasFlag(WriteoffType.User))
            {
                PluginContext.Operations.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToUser(removalType, PluginContext.Operations.GetUsers().First(u => u.IsSessionOpen)), order, orderItem, orderItemModifier,
                    PluginContext.Operations.GetCredentials());
                return;
            }
            throw new NotSupportedException(string.Format("Write-off type '{0}' is not supported.", removalType.WriteoffType));
        }

        /// <summary>
        /// Добавление комментария к блюду.
        /// </summary>
        private void AddProductComment()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.Status == OrderItemStatus.Added);
            PluginContext.Operations.ChangeOrderItemComment("Приготовить без соли.", order, orderItem,
                PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Удаление комментария к блюду.
        /// </summary>
        private void DeleteProductComment()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            var orderItem = order.Items.OfType<IOrderCookingItem>().Last(product => product.Status == OrderItemStatus.Added);
            PluginContext.Operations.DeleteOrderItemComment(order, orderItem,
                PluginContext.Operations.GetCredentials());
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

        private const string deliveryOriginName = "deliveryClub";

        private void CreateDelivery(bool withOriginName)
        {
            var editSession = PluginContext.Operations.CreateEditSession();
            var street = PluginContext.Operations.GetActiveStreets().Last();
            var region = PluginContext.Operations.GetRegions().LastOrDefault();
            var deliveryOperator = PluginContext.Operations.GetUsers().Last();
            var address = new AddressDto
                              {
                                  StreetId = street.Id,
                                  RegionId = region == null ? Guid.Empty : region.Id,
                                  House = "428-с",
                                  Building = "29-m",
                                  Flat = "37",
                              };

            var orderType = PluginContext.Operations.GetOrderTypes().Last(type => type.OrderServiceType == OrderServiceType.DeliveryByCourier);
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
            var client = editSession.CreateClient(Guid.NewGuid(), "Semen",
                                                  new List<PhoneDto> { primaryPhone, secondaryPhone }, null, now, null, null);

            var expectedDeliverTime = now.AddHours(1);
            var deliveryOrder = editSession.CreateDeliveryOrder(1149, now, primaryPhone.PhoneValue,
                address, expectedDeliverTime, orderType, client, deliveryOperator, TimeSpan.FromMinutes(60));
            if (withOriginName)
                editSession.ChangeOrderOriginName(deliveryOriginName, deliveryOrder);
            var guest = editSession.AddOrderGuest("Alex", deliveryOrder);
            editSession.ChangeClientSurname("Petrov", client);
            editSession.ChangeClientNick("Batman", client);
            editSession.ChangeClientEmails(new List<EmailDto> { primaryEmail, secondaryEmail }, client);
            editSession.ChangeClientComment("Customer's Comment", client);
            editSession.ChangeClientAddresses(new List<AddressDto> { address }, 0, client);
            editSession.ChangeClientCardNumber("123456798", client, null, null);
            editSession.ChangeDeliveryComment("Delivery comment", deliveryOrder);
            editSession.ChangeDeliveryEmail(primaryEmail.EmailValue, deliveryOrder);

            SubmitChanges(editSession);
        }

        /// <summary>
        /// Показать доставки с непустым ключом источника
        /// </summary>
        private void ShowDeliveriesWithNotEmptyOriginName()
        {
            var deliveriesWithKey = PluginContext.Operations.GetDeliveryOrders().Where(d => d.OriginName != null)
                                                                    .Select(d => string.Format("{0}: {1}", d.Number, d.OriginName));

            var message = string.Join(",", deliveriesWithKey);
            MessageBox.Show(message);
        }

        /// <summary>
        /// Изменение комментария созданной доставки.
        /// </summary>
        private void ChangeDeliveryComment()
        {
            var createdDeliveryByPlugin = PluginContext.Operations.GetDeliveryOrders().FirstOrDefault(d => d.Number == 1149 && d.DeliveryStatus == DeliveryStatus.New);
            if (createdDeliveryByPlugin == null)
            {
                MessageBox.Show("Please, first call Create Delivery");
                return;
            }

            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.ChangeDeliveryComment(createdDeliveryByPlugin.Comment + "+Changed by test plugin", createdDeliveryByPlugin);

            SubmitChanges(editSession);
        }

        private void SplitOrder()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var result = PluginContext.Operations.NeedToSplitOrderBeforePayment(order);
            if (result == CheckSplitRequiredResult.Disabled)
                return;
            if (result == CheckSplitRequiredResult.Allowed && MessageBox.Show("Split Order by cooking place types?", "Split Orders", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            PluginContext.Operations.SplitOrderBetweenCashRegisters(PluginContext.Operations.GetCredentials(), order);
        }


        /// <summary>
        /// Добавление скидки.
        /// </summary>
        private void AddDiscount()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var discountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && !x.DiscountByFlexibleSum);
            PluginContext.Operations.AddDiscount(discountType, order, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Добавление скидки на произвольную сумму.
        /// </summary>
        private void AddFlexibleSumDiscount()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var discountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.DiscountByFlexibleSum);
            PluginContext.Operations.AddFlexibleSumDiscount(50, discountType, order, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Создание клубной карты с номером "0123456789" и скидкой.
        /// </summary>
        private void CreateDiscountCard()
        {
            var cardDiscountType = PluginContext.Operations.GetDiscountTypes().Last(x => !x.Deleted && x.IsActive && x.CanApplyByDiscountCard);
            PluginContext.Operations.CreateOrUpdateDiscountCard("0123456789", "Semen", null, cardDiscountType);
        }

        /// <summary>
        /// Редактирование клубной карты по номеру "0123456789".
        /// </summary>
        private void UpdateDiscountCard()
        {
            var priceCategory = PluginContext.Operations.GetPriceCategories().Last(x => !x.Deleted);
            PluginContext.Operations.CreateOrUpdateDiscountCard("0123456789", "Semen", priceCategory, null);
        }

        /// <summary>
        /// Добавление скидки и связанного с ним гостя.
        /// </summary>
        private void AddDiscountByCardNumber()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var discountCard = PluginContext.Operations.GetDiscountCards().Last(x => x.DiscountType != null);
            PluginContext.Operations.AddDiscountByCardNumber(discountCard.CardNumber, order, discountCard, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Удаление скидки и связанного с ним гостя.
        /// </summary>
        private void DeleteDiscount()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var cardDiscountItem = order.Discounts.Last(discount => discount.DiscountType.CanApplyByDiscountCard);
            PluginContext.Operations.DeleteDiscount(cardDiscountItem, order, PluginContext.Operations.GetCredentials());
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
            editSession.ChangeSelectiveDiscount(order, discountType, selectedDish, null);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
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
            var product = PluginContext.Operations.GetActiveProducts().Last();
            IProductSize size = null;
            if (product.Scale != null)
            {
                size = product.Scale.DefaultSize;
            }
            var productItem = editSession.AddOrderProductItem(1, PluginContext.Operations.GetActiveProducts().Last(), order, guest, size);
            var firstComboGroupId = Guid.NewGuid();
            var secondComboGroupId = Guid.NewGuid();
            var comboItems = new Dictionary<Guid, IOrderCookingItemStub>
            {
                { firstComboGroupId, productItem },
                { secondComboGroupId, compoundItem}
            };
            editSession.AddOrderCombo(Guid.NewGuid(), "Случайное комбо", 1, 500, Guid.NewGuid(), comboItems, order, guest);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
        }

        /// <summary>
        /// Добавление дополнительной информации в заказ.
        /// </summary>
        private void AddExternalDataInOrder()
        {
            var order = PluginContext.Operations.GetOrders().Last();
            PluginContext.Operations.AddOrderExternalData(PluginName, "Sample plugin external data", order, PluginContext.Operations.GetCredentials());

            var value = PluginContext.Operations.TryGetOrderExternalDataByKey(order.Id, PluginName);
            MessageBox.Show(string.Format("Sample plugin external data value: {0}", value));
        }

        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
        }
    }
}
