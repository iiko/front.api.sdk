using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Data.Orders;
using DpHelper = Resto.Front.Api.CustomerScreen.Helpers.DependencyPropertyHelper<Resto.Front.Api.CustomerScreen.ViewModel.Order>;

namespace Resto.Front.Api.CustomerScreen.ViewModel
{
    internal sealed class Order : DependencyObject
    {
        private static readonly DependencyProperty DiscountPercentProperty = DpHelper.Register(o => o.DiscountPercent);
        private static readonly DependencyProperty IncreasePercentProperty = DpHelper.Register(o => o.IncreasePercent);
        private static readonly DependencyProperty SumBeforeCorrectionsProperty = DpHelper.Register(o => o.SumBeforeCorrections);
        private static readonly DependencyProperty SumAfterCorrectionsProperty = DpHelper.Register(o => o.SumAfterCorrections);
        private static readonly DependencyProperty ChangeSumProperty = DpHelper.Register(o => o.ChangeSum);

        public IOrder OrderSource { get; private set; }
        public ObservableCollection<OrderItem> Items { get; private set; }

        public Order()
        {
            Items = new ObservableCollection<OrderItem>();
        }

        public void Update(IOrder order)
        {
            OrderSource = order;
            Items.Clear();

            var products = order.Items.Where(p => !p.Deleted);
            foreach (var guestProducts in products.GroupBy(product => product.Guest))
            {
                AddItem(guestProducts.Key, Items.Count);
                AddProducts(guestProducts);
            }

            var discount = order.FullSum == 0 ? 0 : Math.Round((100 - ((order.ResultSum * 100) / order.FullSum)), 2);
            DiscountPercent = discount > 0 ? discount : 0;
            IncreasePercent = discount < 0 ? -discount : 0;
            SumBeforeCorrections = order.FullSum;
            SumAfterCorrections = order.ResultSum;
        }

        private void AddProducts(IEnumerable<IOrderRootItem> products)
        {
            foreach (var prod in products.OfType<IOrderProductItem>())
            {
                AddItem(prod, Items.Count);

                var assignedModifiers = prod.AssignedModifiers;
                var zeroAmountModifiers = prod
                    .AvailableGroupModifiers
                    .SelectMany(g => g.Items)
                    .Where(i => i.DefaultAmount > 0
                                && i.HideIfDefaultAmount
                                && !assignedModifiers.Any(p => p.Product.Equals(i.Product)))
                    .ToList();
                foreach (var mod in zeroAmountModifiers)
                {
                    AddItem(mod, Items.Count);
                }
                foreach (var mod in assignedModifiers)
                {
                    // Не показываем модификаторы, количество которых равно количеству по умолчанию
                    if (mod.ProductGroup != null && mod.Cost == 0
                        && prod.AvailableGroupModifiers
                            .SelectMany(g => g.Items)
                            .Any(i => i.Product.Equals(mod.Product)
                                      && i.DefaultAmount == mod.Amount
                                      && i.HideIfDefaultAmount))
                        continue;

                    AddItem(mod, Items.Count);
                }
            }
        }

        public decimal DiscountPercent
        {
            get { return (decimal)GetValue(DiscountPercentProperty); }
            set { SetValue(DiscountPercentProperty, value); }
        }

        public decimal IncreasePercent
        {
            get { return (decimal)GetValue(IncreasePercentProperty); }
            set { SetValue(IncreasePercentProperty, value); }
        }

        public decimal SumBeforeCorrections
        {
            get { return (decimal)GetValue(SumBeforeCorrectionsProperty); }
            set { SetValue(SumBeforeCorrectionsProperty, value); }
        }

        public decimal SumAfterCorrections
        {
            get { return (decimal)GetValue(SumAfterCorrectionsProperty); }
            set { SetValue(SumAfterCorrectionsProperty, value); }
        }


        public decimal ChangeSum
        {
            get { return (decimal)GetValue(ChangeSumProperty); }
            set { SetValue(ChangeSumProperty, value); }
        }

        public void Reset()
        {
            OrderSource = null;
            ClearValue(DiscountPercentProperty);
            ClearValue(IncreasePercentProperty);
            ClearValue(SumBeforeCorrectionsProperty);
            ClearValue(SumAfterCorrectionsProperty);
            Items.Clear();
        }

        public void AddItem(IOrderGuestItem orderItemGuest, int index)
        {
            Items.Insert(index, new OrderItemGuest(orderItemGuest));
        }

        public void AddItem(IOrderProductItem orderItemProduct, int index)
        {
            Items.Insert(index, new OrderItemProduct(orderItemProduct));
        }

        public void AddItem(IOrderModifierItem orderItemModifier, int index)
        {
            Items.Insert(index, new OrderItemModifier(orderItemModifier));
        }

        private void AddItem(IFixedChildModifier zeroAmountModifier, int index)
        {
            Items.Insert(index, new OrderItemModifier(zeroAmountModifier));

        }
    }
}
