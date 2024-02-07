using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.SamplePlugin.WpfHelpers;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class ModifiersWindowViewModel : ViewModelBase
    {
        public delegate void ShowErrorMessage(string messageBoxText, string caption = "Error");
        public event ShowErrorMessage ShowError;

        private ObservableCollection<TreeNodeViewModel> nomenclature;
        public ObservableCollection<TreeNodeViewModel> Nomenclature
        {
            get => nomenclature;
            set
            {
                nomenclature = value;
                RaisePropertyChanged();
            }
        }

        private TreeNodeViewModel selectedNode;
        public TreeNodeViewModel SelectedNode
        {
            get => selectedNode;
            set
            {
                selectedNode = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<OrderItemViewModel> lastOrderItems;
        public ObservableCollection<OrderItemViewModel> LastOrderItems
        {
            get => lastOrderItems;
            set
            {
                lastOrderItems = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand refreshCommand;
        public RelayCommand RefreshNomenclatureCommand =>
            refreshCommand ?? (refreshCommand = new RelayCommand(o => _ = RefreshNomenclature()));

        private RelayCommand refreshLastOrderItemsCommand;

        public RelayCommand RefreshLastOrderItemsCommand =>
            refreshLastOrderItemsCommand ??
            (refreshLastOrderItemsCommand = new RelayCommand(o => _ = RefreshLastOrderItems()));

        private RelayCommand addSimpleModifierToOrderCommand;
        public RelayCommand AddSimpleModifierToOrderCommand =>
            addSimpleModifierToOrderCommand ?? (addSimpleModifierToOrderCommand = new RelayCommand(o => CommandPrepare(AddSimpleModifierToOrder)));

        private RelayCommand addGroupModifierToOrderCommand;
        public RelayCommand AddGroupModifierToOrderCommand =>
            addGroupModifierToOrderCommand ?? (addGroupModifierToOrderCommand = new RelayCommand(o => CommandPrepare(AddGroupModifierToOrder)));

        private RelayCommand deletePrintedOrderModifierCommand;
        public RelayCommand DeletePrintedOrderModifierCommand =>
            deletePrintedOrderModifierCommand ?? (deletePrintedOrderModifierCommand = new RelayCommand(o => CommandPrepare(DeletePrintedOrderModifier)));

        private RelayCommand deleteOrderModifierCommand;
        public RelayCommand DeleteOrderModifierCommand =>
            deleteOrderModifierCommand ?? (deleteOrderModifierCommand = new RelayCommand(o => CommandPrepare(DeleteOrderModifier)));

        private RelayCommand changeOrderModifierItemAmountCommand;
        public RelayCommand ChangeOrderModifierItemAmountCommand =>
            changeOrderModifierItemAmountCommand ?? (changeOrderModifierItemAmountCommand = new RelayCommand(o => CommandPrepare(ChangeOrderModifierItemAmount)));

        private RelayCommand setOrderModifierItemCustomNameCommand;
        public RelayCommand SetOrderModifierItemCustomNameCommand =>
            setOrderModifierItemCustomNameCommand ?? (setOrderModifierItemCustomNameCommand = new RelayCommand(o => CommandPrepare(SetOrderModifierItemCustomName)));

        private RelayCommand addDishWithGroupModifierToOrderCommand;

        public RelayCommand AddDishWithGroupModifierToOrderCommand =>
            addDishWithGroupModifierToOrderCommand ?? (addDishWithGroupModifierToOrderCommand =
                new RelayCommand(o => CommandPrepare(AddDishWithGroupModifierToOrder)));

        public ModifiersWindowViewModel()
        {
        }

        private async Task RefreshNomenclature()
        {
            try
            {
                var nomenclatureRes = await Task.Run(() =>
                {
                    var menu = PluginContext.Operations.GetHierarchicalMenu();
                    var productsVMs = menu.Products.Select(p => new ProductViewModel(p, null));
                    var productGroupsVMs = menu.ProductGroups.Select(pg => new ProductGroupViewModel(pg, null));

                    return productGroupsVMs.OfType<TreeNodeViewModel>().Concat(productsVMs).ToList();
                });
                Nomenclature = new ObservableCollection<TreeNodeViewModel>(nomenclatureRes);
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(ex.ToString());
            }
        }

        private async Task RefreshLastOrderItems()
        {
            try
            {
                var lastOrderItemsVms = await Task.Run(() =>
                {
                    var order = PluginContext.Operations.GetOrders().LastOrDefault(o => o.Status == OrderStatus.New);
                    if (order is null)
                        return Enumerable.Empty<OrderItemViewModel>();

                    var list = new List<OrderItemViewModel>();
                    foreach (var orderRootItem in order.Items)
                    {
                        if (orderRootItem is IOrderProductItem orderProductItem)
                        {
                            list.Add(new OrderProductItemViewModel(orderProductItem));
                            list.AddRange(orderProductItem.AssignedModifiers
                                .Where(assignedModifier =>
                                {
                                    // Фильтруем модификаторы, количество которых равно количеству по умолчанию
                                    return !(assignedModifier.ProductGroup != null && assignedModifier.Cost == 0
                                        && orderProductItem.AvailableGroupModifiers
                                            .SelectMany(gm => gm.Items)
                                            .Any(i => i.Product.Equals(assignedModifier.Product) 
                                                      && i.DefaultAmount == assignedModifier.Amount 
                                                      && i.HideIfDefaultAmount));
                                })
                                .Select(assignedModifier => new OrderModifierItemViewModel(assignedModifier)));
                        }
                    }

                    return list;
                });

                LastOrderItems = new ObservableCollection<OrderItemViewModel>(lastOrderItemsVms);
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(ex.ToString());
            }
        }

        private void CommandPrepare(Action<IOrder, IOrderProductItem> command)
        {
            try
            {
                var order = PluginContext.Operations.GetOrders().LastOrDefault(o => o.Status == OrderStatus.New);
                var orderItem = order?.Items.OfType<IOrderProductItem>().LastOrDefault();
                if (orderItem is null)
                    return;

                command?.Invoke(order, orderItem);
                RefreshLastOrderItemsCommand.Execute(null);
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(ex.ToString());
            }
        }

        private int RandomBetween(int min, int max)
        {
            return Math.Max(1, new Random().Next(min, max + 1));
        }

        private void AddSimpleModifierToOrder(IOrder order, IOrderProductItem orderProductItem)
        {
            // доступный простой модификатор
            var fixedSimpleModifier = orderProductItem.AvailableSimpleModifiers
                .FirstOrDefault(sm => orderProductItem.AssignedModifiers.All(am => am.Product.Id != sm.Product.Id));
            if (fixedSimpleModifier is null)
                return;

            var amount = RandomBetween(fixedSimpleModifier.MinimumAmount, fixedSimpleModifier.MaximumAmount);
            var os = PluginContext.Operations;
            os.AddOrderModifierItem(amount, fixedSimpleModifier.Product, null, order, orderProductItem, os.GetDefaultCredentials());
        }

        private void AddGroupModifierToOrder(IOrder order, IOrderProductItem orderProductItem)
        {
            // доступный групповой модификатор - является фиксированным, не то же самое, что сейчас в номенклатуре
            var fixedGroupModifier = orderProductItem.AvailableGroupModifiers
                .FirstOrDefault(gm => orderProductItem.AssignedModifiers.All(am => am.ProductGroup.Id != gm.ProductGroup.Id));

            // модификатор
            var product = fixedGroupModifier?.Items.LastOrDefault()?.Product;
            if (product is null)
                return;

            var amount = RandomBetween(fixedGroupModifier.MinimumAmount, fixedGroupModifier.MaximumAmount);
            var os = PluginContext.Operations;
            os.AddOrderModifierItem(amount, product, fixedGroupModifier.ProductGroup, order, orderProductItem, os.GetDefaultCredentials());
        }

        private void DeletePrintedOrderModifier(IOrder order, IOrderProductItem orderProductItem)
        {
            // Определяем применённый последний модификатор
            var orderItemModifier = orderProductItem.AssignedModifiers.LastOrDefault();
            if (orderItemModifier is null)
                return;

            var os = PluginContext.Operations;
            var removalType = os.GetActiveRemovalTypes().Last();
            var credentials = os.GetDefaultCredentials();
            const string comment = "SamplePlugin ModifiersView";

            if (removalType.WriteoffType.Equals(WriteoffType.None))
            {
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WithoutWriteoff(removalType), order, orderProductItem, orderItemModifier, credentials);
                return;
            }
            if ((removalType.WriteoffType & WriteoffType.Cafe) != 0)
            {
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToCafe(removalType), order, orderProductItem, orderItemModifier, credentials);
                return;
            }
            if ((removalType.WriteoffType & WriteoffType.Waiter) != 0)
            {
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToWaiter(removalType), order, orderProductItem, orderItemModifier, credentials);
                return;
            }
            if ((removalType.WriteoffType & WriteoffType.User) != 0)
            {
                var user = PluginContext.Operations.GetUsers().First(u => u.IsSessionOpen);
                os.DeletePrintedOrderModifierItem(comment, WriteoffOptions.WriteoffToUser(removalType, user), order, orderProductItem, orderItemModifier, credentials);
            }
        }

        private void DeleteOrderModifier(IOrder order, IOrderProductItem orderProductItem)
        {
            // Определяем применённый последний модификатор
            var orderItemModifier = orderProductItem?.AssignedModifiers.LastOrDefault();
            if (orderItemModifier is null)
                return;

            var os = PluginContext.Operations;
            os.DeleteOrderModifierItem(order, orderProductItem, orderItemModifier, os.GetDefaultCredentials());
        }

        private void ChangeOrderModifierItemAmount(IOrder order, IOrderProductItem orderProductItem)
        {
            // Определяем применённый последний модификатор
            var orderItemModifier = orderProductItem?.AssignedModifiers.LastOrDefault();
            if (orderItemModifier is null)
                return;

            int minimum, maximum;
            if (orderItemModifier.ProductGroup != null)
            {
                // Если группа определена, то модификатор относится к групповым
                var fixedGroupModifier = orderProductItem.AvailableGroupModifiers.FirstOrDefault(gm =>
                    gm.ProductGroup.Id.Equals(orderItemModifier.ProductGroup.Id));
                minimum = fixedGroupModifier?.MinimumAmount ?? 0;
                maximum = fixedGroupModifier?.MaximumAmount ?? 1;
            }
            else
            {
                // В противном случае ищем в доступных простых модификаторах
                var fixedSimpleModifier = orderProductItem.AvailableSimpleModifiers.FirstOrDefault(sm =>
                    sm.Product.Id.Equals(orderItemModifier.Product.Id));
                minimum = fixedSimpleModifier?.MinimumAmount ?? 0;
                maximum = fixedSimpleModifier?.MaximumAmount ?? 1;
            }

            var os = PluginContext.Operations;
            var amount = RandomBetween(minimum, maximum);
            os.ChangeOrderModifierItemAmount(amount, order, orderProductItem, orderItemModifier, os.GetDefaultCredentials());
        }

        private void SetOrderModifierItemCustomName(IOrder order, IOrderProductItem orderProductItem)
        {
            // Определяем применённый последний модификатор
            if (!orderProductItem.AssignedModifiers.Any())
                return;

            var index = new Random().Next(0, orderProductItem.AssignedModifiers.Count);
            var orderItemModifier = orderProductItem.AssignedModifiers[index];
            var os = PluginContext.Operations;
            os.SetOrderModifierItemCustomName($"Eat me {RandomBetween(1, 999)}", order, orderProductItem, orderItemModifier, os.GetDefaultCredentials());
        }

        // блюдо из параметра не используем
        private void AddDishWithGroupModifierToOrder(IOrder order, IOrderProductItem orderProductItem)
        {
            var os = PluginContext.Operations;
            // выбираем продукт с обязательными групповыми модицикатовами.
            var product = os.GetActiveProducts()
                .LastOrDefault(p => p.GetSimpleModifiers(order.PriceCategory).Count == 0 && p.GetGroupModifiers(order.PriceCategory).Any(g => g.MinimumAmount > 0 || g.Items.Any(
                    i => i.DefaultAmount > 0)));

            // вы можете создать свой фильтр для теста, например по атиркулу
            // var product = os.GetActiveProducts().LastOrDefault(p => p.Number == "2000");
            if (product is null)
                return;
            var size = product.Scale?.DefaultSize;
            var session = os.CreateEditSession();
            var productStub = session.AddOrderProductItem(1, product, order, order.Guests[0], size);
            foreach (var group in product.GetGroupModifiers(order.PriceCategory).Where(g => g.MinimumAmount > 0 || g.Items.Any(
                         i => i.DefaultAmount > 0)))
            {
                var count = 0;
                foreach (var modifier in group.Items.OrderBy(i => i.MinimumAmount).ThenBy(i => i.DefaultAmount))
                {
                    var amount = modifier.MinimumAmount;
                    if (amount == 0 || modifier.DefaultAmount != 0)
                        amount = modifier.DefaultAmount;
                    if (amount == 0)
                        amount = Math.Min(modifier.MaximumAmount, group.MaximumAmount - count);

                    session.AddOrderModifierItem(amount, modifier.Product, group.ProductGroup, order, productStub);
                    count += amount;
                    if (count >= group.MaximumAmount)
                        break;
                }
            }
            os.SubmitChanges(session);
        }
    }
}
