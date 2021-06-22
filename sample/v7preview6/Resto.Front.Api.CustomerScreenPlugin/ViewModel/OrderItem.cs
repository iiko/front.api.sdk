using System.Windows;
using Resto.Front.Api.Data.Common;

namespace Resto.Front.Api.CustomerScreen.ViewModel
{
    internal abstract class OrderItem : DependencyObject
    {
        public IEntity Source { get; private set; }
        public string Name { get; private set; }

        protected OrderItem(IEntity source, string name)
        {
            Source = source;
            Name = name;
        }
    }
}
