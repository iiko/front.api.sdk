namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal class LoadingTreeNodeViewModel : TreeNodeViewModel
    {
        public override string DisplayText => "Loading…";

        public LoadingTreeNodeViewModel(TreeNodeViewModel parent) : base(parent, false)
        {
        }

        public LoadingTreeNodeViewModel()
        {
        }
    }
}
