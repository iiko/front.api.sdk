using System;
using Resto.Front.Api.SamplePlugin.WpfHelpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal abstract class TreeNodeViewModel : ViewModelBase
    {
        private readonly Lazy<LoadingTreeNodeViewModel> lazyLoadingNode;
        protected LoadingTreeNodeViewModel LoadingNode => lazyLoadingNode.Value;

        private readonly TreeNodeViewModel parentNode;
        public TreeNodeViewModel ParentNode => parentNode;

        public ObservableCollection<TreeNodeViewModel> ChildrenNodes { get; } = new ObservableCollection<TreeNodeViewModel>();

        public virtual string DisplayText => string.Empty;

        public bool IsNodeLoaded => !ChildrenNodes.Contains(LoadingNode);

        public bool IsNodeLoading { get; private set; }

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (value == isExpanded)
                    return;

                if (value && parentNode != null)
                    parentNode.isExpanded = true;

                isExpanded = value;
                RaisePropertyChanged();

                if (!value || IsNodeLoaded || IsNodeLoading)
                    return;

                IsNodeLoading = true;
                Task.Run(() => _ = UpdateChildrenNodesAsync()).ContinueWith(t => IsNodeLoading = false, TaskScheduler.Current);
            }
        }

        private TreeNodeViewModel()
        {
            lazyLoadingNode = new Lazy<LoadingTreeNodeViewModel>(() => new LoadingTreeNodeViewModel(this));
        }

        protected TreeNodeViewModel(TreeNodeViewModel parent = null, bool addLoadingNode = true) : this()
        {
            parentNode = parent;

            if (addLoadingNode)
                ChildrenNodes.Add(LoadingNode);
        }

        protected virtual Task<IReadOnlyCollection<TreeNodeViewModel>> GetChildrenNodesAsync()
        {
            return Task.FromResult(new List<TreeNodeViewModel>() as IReadOnlyCollection<TreeNodeViewModel>);
        }

        public async Task UpdateChildrenNodesAsync()
        {
            try
            {
                var nodes = await GetChildrenNodesAsync();

                await DispatcherHelper.UiDispatcher.InvokeAsync(() =>
                {
                    ChildrenNodes.Clear();
                    foreach (var node in nodes)
                    {
                        ChildrenNodes.Add(node);
                    }
                });
            }
            catch
            {
            }
        }
    }
}
