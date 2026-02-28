using Resto.Front.Api.CustomerScreen.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Data.Orders;
using System.Globalization;
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
        // ReSharper disable once MemberCanBePrivate.Global
        public ObservableCollection<OrderItem> Items { get; }

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
                AddGuestItem(guestProducts.Key, Items.Count);
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
            foreach (var product in products)
            {
                // Если используется блюдо c размерами.
                if (product is IOrderCompoundItem compoundItem)
                {
                    AddCompoundItem(compoundItem, Items.Count);

                    var assignedModifiers = compoundItem.CommonModifiers;
                    var zeroAmountModifiers = compoundItem
                        .AvailableComponentGroupModifiers
                        .SelectMany(g => g.Items)
                        .Where(i => i.DefaultAmount > 0
                                    && !assignedModifiers.Any(p => p.Product.Equals(i.Product)))
                        .ToList();
                    foreach (var modifierItem in zeroAmountModifiers)
                    {
                        AddFixedChildModifierItem(modifierItem, Items.Count);
                    }

                    foreach (var modifierItem in assignedModifiers)
                    {
                        // Не показываем модификаторы, количество которых равно количеству по умолчанию
                        if (modifierItem.ProductGroup != null && modifierItem.Cost == 0
                                                     && compoundItem.AvailableCommonGroupModifiers
                                                         .SelectMany(g => g.Items)
                                                         .Any(i => i.Product.Equals(modifierItem.Product)
                                                                   && i.DefaultAmount == modifierItem.Amount
                                                                   && i.HideIfDefaultAmount))
                            continue;
                        var modifierSettings = compoundItem.AvailableCommonGroupModifiers.SelectMany(x => x.Items).FirstOrDefault(x => x.Product.Equals(modifierItem.Product));
                        var amountString = modifierSettings != null ?
                            AmountHelper.CalculateModifierAmountString(modifierItem.Amount, modifierSettings.DefaultAmount, modifierSettings.HideIfDefaultAmount, isPaid: false,
                                modifierItem.AmountIndependentOfParentAmount)
                            : modifierItem.Amount.ToString(CultureInfo.CurrentCulture);
                        AddModifierItem(modifierItem, amountString, Items.Count);
                    }
                }

                // Если обычное блюдо с модификаторами.
                if (product is IOrderProductItem productItem)
                {
                    AddProductItem(productItem, Items.Count);

                    var assignedModifiers = productItem.AssignedModifiers;
                    var zeroAmountModifiers = productItem
                        .AvailableGroupModifiers
                        .SelectMany(g => g.Items)
                        .Where(i => i.DefaultAmount > 0
                                    && i.HideIfDefaultAmount
                                    && !assignedModifiers.Any(p => p.Product.Equals(i.Product)))
                        .ToList();
                    foreach (var modifierItem in zeroAmountModifiers)
                    {
                        if (!modifierItem.HideIfDefaultAmount)
                            AddFixedChildModifierItem(modifierItem, Items.Count);
                    }

                    foreach (var modifierItem in assignedModifiers)
                    {
                        // Не показываем модификаторы, количество которых равно количеству по умолчанию
                        if (modifierItem.ProductGroup != null && modifierItem.Cost == 0
                                                     && productItem.AvailableGroupModifiers
                                                         .SelectMany(g => g.Items)
                                                         .Any(i => i.Product.Equals(modifierItem.Product)
                                                                   && i.DefaultAmount == modifierItem.Amount
                                                                   && i.HideIfDefaultAmount))
                            continue;

                        var modifierSettings = productItem.AvailableGroupModifiers.SelectMany(x => x.Items).FirstOrDefault(x => x.Product.Equals(modifierItem.Product));
                        var amountString = modifierSettings != null ?
                            AmountHelper.CalculateModifierAmountString(modifierItem.Amount, modifierSettings.DefaultAmount, modifierSettings.HideIfDefaultAmount, isPaid: false,
                                modifierItem.AmountIndependentOfParentAmount)
                            : modifierItem.Amount.ToString(CultureInfo.CurrentCulture);
                        AddModifierItem(modifierItem, amountString, Items.Count);
                    }
                }
            }
        }

        private decimal DiscountPercent
        {
            get => (decimal)GetValue(DiscountPercentProperty);
            set => SetValue(DiscountPercentProperty, value);
        }

        private decimal IncreasePercent
        {
            get => (decimal)GetValue(IncreasePercentProperty);
            set => SetValue(IncreasePercentProperty, value);
        }

        private decimal SumBeforeCorrections
        {
            get => (decimal)GetValue(SumBeforeCorrectionsProperty);
            set => SetValue(SumBeforeCorrectionsProperty, value);
        }

        private decimal SumAfterCorrections
        {
            get => (decimal)GetValue(SumAfterCorrectionsProperty);
            set => SetValue(SumAfterCorrectionsProperty, value);
        }


        public decimal ChangeSum
        {
            get => (decimal)GetValue(ChangeSumProperty);
            set => SetValue(ChangeSumProperty, value);
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

        private void AddGuestItem(IOrderGuestItem orderItemGuest, int index)
        {
            if (!string.IsNullOrEmpty(orderItemGuest.Name))
                Items.Insert(index, new OrderItemGuest(orderItemGuest));
        }

        private void AddCompoundItem(IOrderCompoundItem orderItemCompound, int index)
        {
            var n = new OrderItemCompound(orderItemCompound);
            Items.Insert(index, n);
        }

        private void AddProductItem(IOrderProductItem orderItemProduct, int index)
        {
            var n = new OrderItemProduct(orderItemProduct);
            Items.Insert(index, n);
        }

        private void AddModifierItem(IOrderModifierItem orderItemModifier, string amountString, int index)
        {
            Items.Insert(index, new OrderItemModifier(orderItemModifier, amountString));
        }

        private void AddFixedChildModifierItem(IFixedChildModifier zeroAmountModifier, int index)
        {
            Items.Insert(index, new OrderItemModifier(zeroAmountModifier));

        }
    }
}
