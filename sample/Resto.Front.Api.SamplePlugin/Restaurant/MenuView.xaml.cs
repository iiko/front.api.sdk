using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class MenuView
    {
        public MenuView()
        {
            InitializeComponent();

            ReloadRestaurant();
        }

        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            ReloadRestaurant();
        }

        private void ReloadRestaurant()
        {
            var menu = PluginContext.Operations.GetHierarchicalMenu();
            treeMenu.ItemsSource = ConcatProductsAndGroups(menu.Products, menu.ProductGroups).ToList();
        }

        [NotNull]
        private IEnumerable<object> ConcatProductsAndGroups([NotNull] IEnumerable<IProduct> products, [NotNull] IEnumerable<IProductGroup> productGroups)
        {
            foreach (var productGroupModel in productGroups.Select(productGroup => new ProductGroupModel(productGroup)))
                yield return productGroupModel;

            var remainingAmounts = PluginContext.Operations.GetProductsRemainingAmounts();
            foreach (var product in products)
            {
                var remainingAmount = remainingAmounts.ContainsKey(product) ? remainingAmounts[product] : (decimal?)null;
                var isSellingRestricted = PluginContext.Operations.IsProductSellingRestricted(product);
                var includedInMenuSections = PluginContext.Operations.GetIncludedInMenuSectionsByProduct(product);
                yield return new ProductModel(product, includedInMenuSections, remainingAmount, isSellingRestricted);
            }
        }

        private void OnProductGroupExpanded(object sender, RoutedEventArgs e)
        {
            var productGroupModel = (ProductGroupModel)((TreeViewItem)sender).DataContext;
            if (!productGroupModel.HasFakeItem)
                return;

            var childProducts = PluginContext.Operations.GetChildProductsByProductGroup(productGroupModel.ProductGroup);
            var childGroups = PluginContext.Operations.GetChildGroupsByProductGroup(productGroupModel.ProductGroup);
            productGroupModel.ReplaceItems(ConcatProductsAndGroups(childProducts, childGroups));
        }

        private void OnProductExpanded(object sender, RoutedEventArgs e)
        {
            var productModel = (ProductModel)((TreeViewItem)sender).DataContext;
            if (!productModel.HasFakeItem)
                return;

            var groupModifiers = PluginContext.Operations.GetGroupModifiersByProduct(productModel.Product);
            var simpleModifiers = PluginContext.Operations.GetSimpleModifiersByProduct(productModel.Product);
            productModel.ReplaceItems(groupModifiers.Cast<object>().Concat(simpleModifiers));
        }
    }
}
