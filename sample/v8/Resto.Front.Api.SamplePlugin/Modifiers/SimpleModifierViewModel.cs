using Resto.Front.Api.Data.Assortment;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class SimpleModifierViewModel : TreeNodeViewModel
    {
        private readonly ISimpleModifier simpleModifier;

        public override string DisplayText => simpleModifier.Product.Name;
        public int DefaultAmount => simpleModifier.DefaultAmount;
        public int FreeOfChargeAmount => simpleModifier.FreeOfChargeAmount;
        public int MaximumAmount => simpleModifier.MaximumAmount;
        public int MenuIndex => simpleModifier.MenuIndex;
        public int MinimumAmount => simpleModifier.MinimumAmount;
        public bool AmountIndependentOfParentAmount => simpleModifier.AmountIndependentOfParentAmount;

        public SimpleModifierViewModel(ISimpleModifier simpleModifier, TreeNodeViewModel parent) : base(parent, false)
        {
            this.simpleModifier = simpleModifier;
        }
    }
}
