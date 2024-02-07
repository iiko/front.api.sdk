using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Orders;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class ProductViewModel : TreeNodeViewModel
    {
        private readonly IProduct product;

        public override string DisplayText => product.Name;
        public bool CanSetOpenPrice => product.CanSetOpenPrice;
        public string Description => product.Description;
        public string DescriptionForeign => product.DescriptionForeign;
        public string FastCode => product.FastCode;
        public string ForeignName => product.ForeignName;
        public string FullName => product.FullName;
        public string Number => product.Number;
        public string KitchenName => product.KitchenName;
        public string MeasuringUnitName => product.MeasuringUnitName;
        public int MenuIndex => product.MenuIndex;
        public Color? BackgroundColor => product.BackgroundColor;
        public Color? FontColor => product.FontColor;
        public bool HasMenuImage => product.HasMenuImage;
        public ProductType Type => product.Type;
        public decimal Price => product.Price;
        public decimal FoodValueFat => product.FoodValueFat;
        public decimal FoodValueProtein => product.FoodValueProtein;
        public decimal FoodValueCarbohydrate => product.FoodValueCarbohydrate;
        public decimal FoodValueCaloricity => product.FoodValueCaloricity;
        public OrderItemCourse DefaultCourse => product.DefaultCourse;
        public bool IsActive => product.IsActive;
        public TimeSpan? ExpirationTime => product.ExpirationTime;
        public decimal? TaxPercent => product.TaxCategory?.VatPercent;
        public bool ImmediateCookingStart => product.ImmediateCookingStart;
        public bool UseBalanceForSell => product.UseBalanceForSell;
        public Guid Id => product.Id;

        public ProductViewModel(IProduct product, TreeNodeViewModel parent) : base(parent)
        {
            this.product = product;
        }

        protected override Task<IReadOnlyCollection<TreeNodeViewModel>> GetChildrenNodesAsync()
        {
            // Ценовая категория отделения, в котором расположен стол по умолчанию
            var priceCategory = PluginContext.Operations.GetHostTerminalsGroup().PriceCategory;

            // Получаем групповые и простые модификаторы
            var groupModifiers = product.GetGroupModifiers(priceCategory).Select(gm => new GroupModifierViewModel(gm, this));
            var simpleModifiers = product.GetSimpleModifiers(priceCategory).Select(sm => new SimpleModifierViewModel(sm, this));

            // Для наглядности складываем в один список
            var allModifiers = groupModifiers.OfType<TreeNodeViewModel>().Concat(simpleModifiers).ToList() as IReadOnlyCollection<TreeNodeViewModel>;

            return Task.FromResult(allModifiers);
        }
    }
}
