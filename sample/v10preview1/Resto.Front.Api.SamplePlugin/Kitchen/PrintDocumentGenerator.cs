using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.Kitchen;
using Resto.Front.Api.Data.Print;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    internal static class PrintDocumentGenerator
    {
        public static Document GenerateKitchenDocument([NotNull] IKitchenOrder order)
        {
            return new Document
            {
                Markup = new XElement(Tags.Doc, GenerateKitchenDocElements(order))
            };
        }

        private static IEnumerable<XElement> GenerateKitchenDocElements([NotNull] IKitchenOrder order)
        {
            yield return new XElement(Tags.MediumFont,
                new XElement(Tags.Pair,
                    new XAttribute(Data.Cheques.Attributes.Left, order.OrderType?.Name ?? "-"),
                    new XAttribute(Data.Cheques.Attributes.Right, $"ЗАКАЗ: {order.Number}")));
            yield return new XElement(Tags.Line,
                new XAttribute(Data.Cheques.Attributes.Symbols, "="));

            yield return new XElement(Tags.Table,
                new XElement(Tags.Columns,
                    new XElement(Tags.Column,
                        new XAttribute(Data.Cheques.Attributes.Align, AttributeValues.Right),
                        new XAttribute(Data.Cheques.Attributes.AutoWidth, AttributeValues.Empty)),
                    new XElement(Tags.Column,
                        new XAttribute(Data.Cheques.Attributes.Width, 1)),
                    new XElement(Tags.Column)),
                new XElement(Tags.Cells,
                    GetItemPositions(order)
                    ));

            yield return new XElement(Tags.Center, new XElement(Tags.MediumFont, "СТОЛ"));
            yield return new XElement(Tags.Center, new XElement(Tags.LargeFont, order.Table.Number.ToString("###")));

            yield return new XElement(Tags.Line,
                new XAttribute(Data.Cheques.Attributes.Symbols, "="));

            yield return new XElement(Tags.Pair,
                new XAttribute(Data.Cheques.Attributes.Left, order.OriginName ?? "-"),
                new XAttribute(Data.Cheques.Attributes.Right, DateTime.Now.ToString("dd/MM/yyy HH:mm:ss", CultureInfo.InvariantCulture)));

            yield return new XElement(Tags.NewParagraph);
        }

        private static IEnumerable<XElement> GetItemPositions([NotNull] IKitchenOrder order)
        {
            foreach (var group in order.Items.Where(i => !i.Deleted).GroupBy(i => i.GetProductCategory()).OrderBy(g => g.Key?.Name))
            {
                if (group.Key != null)
                    yield return new XElement(Tags.Cell, group.Key.Name,
                        new XAttribute(Data.Cheques.Attributes.ColumnSpan, 3));

                foreach (var (itemName, item) in group.Select(i => (itemName: i.GetCookingItemName(), item: i)).OrderBy(i => i.itemName))
                {
                    var itemAmount = item.GetCookingItemAmount();

                    yield return new XElement(Tags.TextCell, itemAmount.ToAmountString());
                    yield return new XElement(Tags.TextCell);
                    yield return new XElement(Tags.TextCell, itemName);

                    // Обычное блюдо.
                    if (item is IKitchenOrderProductItem orderProductItem)
                    {
                        foreach (var (modifierName, modifierAmount, modifierAmountIndependentOfParentAmount) in orderProductItem.Modifiers
                                     .Where(m => !m.Deleted && !m.IsHidden)
                                     .Select(m => (
                                         modifierName: m.Product.Name,
                                         modifierAmount: m.AmountIndependentOfParentAmount ? m.Amount : m.Amount / itemAmount,
                                         modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount))
                                     .Concat(orderProductItem.ZeroAmountModifiersData
                                         .Select(m => (
                                             modifierName: m.Product.Name,
                                             modifierAmount: 0m,
                                             modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount))))
                        {
                            foreach (var modifierPosition in GetModifierPositions(modifierName, string.Empty, modifierAmount, modifierAmountIndependentOfParentAmount))
                                yield return modifierPosition;
                        }
                        continue;
                    }

                    // Пицца.
                    var compoundOrderItem = (IKitchenOrderCompoundItem)item;
                    var isDivided = compoundOrderItem.SecondaryComponent != null;

                    // Целая пицца.
                    if (!isDivided)
                    {
                        foreach (var (modifierName, modifierAmount, modifierAmountIndependentOfParentAmount) in
                                 compoundOrderItem.CommonModifiers
                                     .Where(m => !m.Deleted && !m.IsHidden)
                                     .Select(m => (
                                         modifierName: m.Product.Name,
                                         modifierAmount: m.AmountIndependentOfParentAmount ? m.Amount : m.Amount / itemAmount,
                                         modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount))
                                     .Concat(compoundOrderItem.ZeroAmountCommonModifiersData
                                         .Select(m => (
                                             modifierName: m.Product.Name,
                                             modifierAmount: 0m,
                                             modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount)))
                                     .Concat(compoundOrderItem.PrimaryComponent.Modifiers
                                         .Where(m => !m.Deleted && !m.IsHidden)
                                         .Select(m => (
                                             modifierName: m.Product.Name,
                                             modifierAmount: m.AmountIndependentOfParentAmount ? m.Amount : m.Amount / itemAmount,
                                             modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount)))
                                     .Concat(compoundOrderItem.PrimaryComponent.ZeroAmountModifiersData
                                         .Select(m => (
                                             modifierName: m.Product.Name,
                                             modifierAmount: 0m,
                                             modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount)))
                                     .OrderBy(m => m.modifierName)
                                 )
                        {
                            foreach (var modifierPosition in GetModifierPositions(modifierName, string.Empty, modifierAmount, modifierAmountIndependentOfParentAmount))
                                yield return modifierPosition;
                        }
                        continue;
                    }

                    // Разделенная пицца.
                    const string indent = "  ";

                    foreach (var (modifierName, modifierAmount, modifierAmountIndependentOfParentAmount) in compoundOrderItem.CommonModifiers
                                 .Where(m => !m.Deleted && !m.IsHidden)
                                 .Select(m => (
                                     modifierName: m.Product.Name,
                                     modifierAmount: m.AmountIndependentOfParentAmount ? m.Amount : m.Amount / itemAmount,
                                     modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount))
                                 .Concat(compoundOrderItem.ZeroAmountCommonModifiersData
                                     .Select(m => (
                                         modifierName: m.Product.Name,
                                         modifierAmount: 0m,
                                         modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount)))
                                 .OrderBy(m => m.modifierName)
                             )
                    {
                        foreach (var modifierPosition in GetModifierPositions(modifierName, indent, modifierAmount, modifierAmountIndependentOfParentAmount))
                            yield return modifierPosition;
                    }

                    // Левая половинка пиццы.
                    yield return new XElement(Tags.TextCell);
                    yield return new XElement(Tags.TextCell);
                    yield return new XElement(Tags.TextCell, $"  1/2 {compoundOrderItem.PrimaryComponent.Product.Name}");

                    foreach (var (modifierName, modifierAmount, modifierAmountIndependentOfParentAmount) in compoundOrderItem.PrimaryComponent.Modifiers
                                 .Where(m => !m.Deleted && !m.IsHidden)
                                 .Select(m => (
                                     modifierName: m.Product.Name,
                                     modifierAmount: m.AmountIndependentOfParentAmount ? m.Amount : m.Amount / (itemAmount / 2),
                                     modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount))
                                 .Concat(compoundOrderItem.PrimaryComponent.ZeroAmountModifiersData
                                     .Select(m => (
                                         modifierName: m.Product.Name,
                                         modifierAmount: 0m,
                                         modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount)))
                                 .OrderBy(m => m.modifierName)
                             )
                    {
                        foreach (var modifierPosition in GetModifierPositions(modifierName, indent, modifierAmount, modifierAmountIndependentOfParentAmount))
                            yield return modifierPosition;
                    }

                    // Правая половинка пиццы.
                    yield return new XElement(Tags.TextCell);
                    yield return new XElement(Tags.TextCell);
                    yield return new XElement(Tags.TextCell, $"  1/2 {compoundOrderItem.SecondaryComponent.Product.Name}");

                    foreach (var (modifierName, modifierAmount, modifierAmountIndependentOfParentAmount) in compoundOrderItem.SecondaryComponent.Modifiers
                                 .Where(m => !m.Deleted && !m.IsHidden)
                                 .Select(m => (
                                     modifierName: m.Product.Name,
                                     modifierAmount: m.AmountIndependentOfParentAmount ? m.Amount : m.Amount / (itemAmount / 2),
                                     modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount))
                                 .Concat(compoundOrderItem.SecondaryComponent.ZeroAmountModifiersData
                                     .Select(m => (
                                         modifierName: m.Product.Name,
                                         modifierAmount: 0m,
                                         modifierAmountIndependentOfParentAmount: m.AmountIndependentOfParentAmount)))
                                 .OrderBy(m => m.modifierName)
                             )
                    {
                        foreach (var modifierPosition in GetModifierPositions(modifierName, indent, modifierAmount, modifierAmountIndependentOfParentAmount))
                            yield return modifierPosition;
                    }
                }

                yield return new XElement(Tags.LineCell);
            }
        }

        private static IEnumerable<XElement> GetModifierPositions(string name, string indent, decimal amount, bool amountIndependentOfParentAmount)
        {
            yield return new XElement(Tags.TextCell);
            yield return new XElement(Tags.TextCell);

            if (amountIndependentOfParentAmount)
                yield return new XElement(Tags.TextCell, $"{indent}  +{amount.ToAmountString()} {name}");
            else if (amount != 1m)
                yield return new XElement(Tags.TextCell, $"{indent}  x{amount.ToAmountString()} {name}");
            else
                yield return new XElement(Tags.TextCell, $"{indent}  {name}");
        }

        [Pure]
        private static string ToAmountString(this decimal value) => value.ToString("0.###");

        private static IProductCategory GetProductCategory(this IKitchenOrderCookingItem cookingItem)
        {
            if (cookingItem is IKitchenOrderProductItem orderProductItem)
                return orderProductItem.Product.Category;
            return ((IKitchenOrderCompoundItem)cookingItem).PrimaryComponent.Product.Category;
        }
    }
}
