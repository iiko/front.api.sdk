using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;

namespace Resto.Front.Api.SamplePlugin.PreliminaryOrders
{
    internal sealed partial class OrderEditView
    {
        private readonly OrderModel orderModel;
        private readonly Window window;

        public OrderEditView(Window window, OrderModel orderModel)
        {
            this.window = window;
            this.orderModel = orderModel;

            InitializeComponent();

            DataContext = orderModel;

            lstProducts.ItemsSource = PluginContext.Operations.GetHierarchicalMenu().Products
                .Where(product => product.Type == ProductType.Dish ||
                                  product.Type == ProductType.Modifier ||
                                  product.Type == ProductType.Goods)
                .ToList();
        }

        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            window.DialogResult = true;
        }

        private void ButtonAddProductItemClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = lstProducts.SelectedItem;
            if (selectedItem == null)
                return;

            var selectedProduct = (IProduct)selectedItem;
            var productItemModel = new ProductItemModel(selectedProduct, 1m, selectedProduct.Scale != null ? selectedProduct.Scale.DefaultSize : null);


            orderModel.AddProductItem(productItemModel);
        }

        [CanBeNull]
        private ProductItemModel GetSelectedProductItem()
        {
            return lstItems.SelectedItem as ProductItemModel;
        }

        [CanBeNull]
        private ModifierItemModel GetSelectedModifierItem()
        {
            return lstItems.SelectedItem as ModifierItemModel;
        }

        private void ButtonDeleteItemClick(object sender, RoutedEventArgs e)
        {
            var selectedProductItem = GetSelectedProductItem();
            if (selectedProductItem != null)
            {
                orderModel.RemoveProductItem(selectedProductItem);
                return;
            }

            var selectedModifierItem = GetSelectedModifierItem();
            if (selectedModifierItem != null)
                selectedModifierItem.Parent.RemoveModifierItem(selectedModifierItem);
        }

        private void ButtonAddModifierItemClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = lstModifiers.SelectedItem;
            if (selectedItem == null)
                return;

            IProduct modifierDish;
            bool isChildModifier;

            var childModifier = selectedItem as IChildModifier;
            if (childModifier != null)
            {
                Debug.Assert(childModifier.Product.Type == ProductType.Modifier);
                modifierDish = childModifier.Product;
                isChildModifier = true;
            }
            else
            {
                var simpleModifier = (ISimpleModifier)selectedItem;
                modifierDish = simpleModifier.Product;
                isChildModifier = false;
            }

            var selectedProductItem = GetSelectedProductItem();
            Debug.Assert(selectedProductItem != null);

            var existingModifier = selectedProductItem.Children
                .SingleOrDefault(m => m.IsChildModifier == isChildModifier && m.Dish.Id == modifierDish.Id);


            if (existingModifier != null)
            {
                existingModifier.Amount++;
            }
            else
            {
                var modifierItemModel = new ModifierItemModel
                                        {
                                            Dish = modifierDish,
                                            Amount = 1,
                                            IsChildModifier = isChildModifier
                                        };
                selectedProductItem.AddModifierItem(modifierItemModel);
            }
        }

        private void LstItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProductItem = GetSelectedProductItem();
            if (selectedProductItem == null)
            {
                lstModifiers.ItemsSource = null;
                lstModifiers.IsEnabled = false;
                return;
            }

            var dish = selectedProductItem.Dish;

            var groupModifiers = PluginContext.Operations.GetGroupModifiersByProduct(dish);
            var simpleModifiers = PluginContext.Operations.GetSimpleModifiersByProduct(dish);

            lstModifiers.ItemsSource = groupModifiers
                .SelectMany(gm => gm.Items)
                .Cast<object>()
                .Concat(simpleModifiers)
                .ToList();

            lstModifiers.IsEnabled = true;
        }
    }
}