using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Resto.Front.Api.Data.Assortment;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class GroupModifierViewModel : TreeNodeViewModel
    {
        private readonly IGroupModifier groupModifier;

        public override string DisplayText => groupModifier.ProductGroup.Name;
        public int Count => groupModifier.Items.Count;
        public int FreeOfChargeAmount => groupModifier.FreeOfChargeAmount;
        public int MaximumAmount => groupModifier.MaximumAmount;
        public int MinimumAmount => groupModifier.MinimumAmount;
        public int MenuIndex => groupModifier.MenuIndex;

        public GroupModifierViewModel(IGroupModifier groupModifier, TreeNodeViewModel parent) : base(parent)
        {
            this.groupModifier = groupModifier;
        }

        protected override Task<IReadOnlyCollection<TreeNodeViewModel>> GetChildrenNodesAsync()
        {
            // В групповых модификаторах есть обычные модификаторы, но это не то же самое что ISimpleModifier
            var childModifiersVMs = groupModifier.Items.Select(cm => new ChildModifierViewModel(cm, this)).ToList() as IReadOnlyCollection<TreeNodeViewModel>;

            return Task.FromResult(childModifiersVMs);
        }
    }
}
