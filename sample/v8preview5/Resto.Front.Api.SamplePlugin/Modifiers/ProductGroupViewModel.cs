using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Resto.Front.Api.Data.Assortment;
using System;
using System.Drawing;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class ProductGroupViewModel : TreeNodeViewModel
    {
        private readonly IProductGroup productGroup;
        public override string DisplayText => productGroup.Name;
        public string FastCode => productGroup.FastCode;
        public string Number => productGroup.Number;
        public Color? BackgroundColor => productGroup.BackgroundColor;
        public Color? FontColor => productGroup.FontColor;
        public bool HasMenuImage => productGroup.HasMenuImage;
        public int MenuIndex => productGroup.MenuIndex;
        public Guid Id => productGroup.Id;

        public ProductGroupViewModel(IProductGroup productGroup, TreeNodeViewModel parent) : base(parent)
        {
            this.productGroup = productGroup;
        }

        protected override Task<IReadOnlyCollection<TreeNodeViewModel>> GetChildrenNodesAsync()
        {
            // Получение продуктов и групп по этой группе
            var childProducts = PluginContext.Operations.GetChildProductsByProductGroup(productGroup).Select(p => new ProductViewModel(p, this));
            var childGroups = PluginContext.Operations.GetChildGroupsByProductGroup(productGroup).Select(pg => new ProductGroupViewModel(pg, this));

            var allItems = childGroups.OfType<TreeNodeViewModel>().Concat(childProducts).ToList() as IReadOnlyCollection<TreeNodeViewModel>;

            return Task.FromResult(allItems);
        }
    }
}
