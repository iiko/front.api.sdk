using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    internal abstract class MenuItemModel
    {
        private readonly object fakeItem = new object();
        private readonly ObservableCollection<object> items = new ObservableCollection<object>();

        [UsedImplicitly] // используется через Binding
        [NotNull]
        public ReadOnlyObservableCollection<object> Items { get; private set; }

        public bool HasFakeItem
        {
            get { return items.Count == 1 && items.Single() == fakeItem; }
        }

        protected MenuItemModel()
        {
            items.Add(fakeItem);
            Items = new ReadOnlyObservableCollection<object>(items);
        }

        public void ReplaceItems([NotNull] IEnumerable<object> newItems)
        {
            items.Clear();
            newItems.ForEach(items.Add);
        }
    }
}
