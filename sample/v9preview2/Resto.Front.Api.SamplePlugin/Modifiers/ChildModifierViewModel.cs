using Resto.Front.Api.Data.Assortment;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class ChildModifierViewModel : TreeNodeViewModel
    {
        private readonly IChildModifier childModifier;

        public override string DisplayText => childModifier.Product.Name;
        public bool AmountIndependentOfParentAmount => childModifier.AmountIndependentOfParentAmount;
        public bool HideIfDefaultAmount => childModifier.HideIfDefaultAmount;
        public int DefaultAmount => childModifier.DefaultAmount;
        public int FreeOfChargeAmount => childModifier.FreeOfChargeAmount;
        public int MaximumAmount => childModifier.MaximumAmount;
        public int MinimumAmount => childModifier.MinimumAmount;

        public ChildModifierViewModel(IChildModifier childModifier, TreeNodeViewModel parent) : base(parent, false)
        {
            this.childModifier = childModifier;
        }
    }
}
