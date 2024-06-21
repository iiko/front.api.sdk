using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Organization.Sections;

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

            var remainingAmounts = PluginContext.Operations.GetStopListProductsRemainingAmounts();
            foreach (var product in products)
            {
                var key = new ProductAndSize(product, null);
                var remainingAmount = remainingAmounts.ContainsKey(key) ? remainingAmounts[key] : (decimal?)null;
                var isSellingRestricted = PluginContext.Operations.IsStopListProductSellingRestricted(product, null);
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

        /// Для правильного отображения списка модификаторов нужно указывать правильную ценовую категорию.
        /// В случае, если в заведении ценовые категории не используются, можно передать <c>null</c>.
        /// Получить ЦК также можно из
        /// <see cref="IRestaurantSection.DefaultPriceCategory" /> - ЦК, назначенная отделению;
        /// <see cref="Data.Organization.ITerminalsGroup.PriceCategory" /> - ЦК отделения, которому принадлежит стол по умолчанию;
        /// <see cref="Data.Orders.IOrder.PriceCategory" /> - ЦК, назначенная заказу;
        /// <see cref="IOperationService.GetPriceCategories" /> - список ценовых категорий;
        /// <see cref="IOperationService.GetPriceCategoryById" /> - получение ЦК по id.
        private void OnProductExpanded(object sender, RoutedEventArgs e)
        {
            var productModel = (ProductModel)((TreeViewItem)sender).DataContext;
            if (!productModel.HasFakeItem)
                return;

            // Ценовая категория отделения, в котором расположен стол по умолчанию
            var priceCategory = PluginContext.Operations.GetHostTerminalsGroup().PriceCategory;

            var groupModifiers = productModel.Product.GetGroupModifiers(priceCategory);
            var simpleModifiers = productModel.Product.GetSimpleModifiers(priceCategory);
            productModel.ReplaceItems(groupModifiers.Cast<object>().Concat(simpleModifiers));
        }
    }
}
