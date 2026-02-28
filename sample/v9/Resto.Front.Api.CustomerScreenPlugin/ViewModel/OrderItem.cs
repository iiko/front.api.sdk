using System.Windows;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.Orders;

namespace Resto.Front.Api.CustomerScreen.ViewModel
{
    internal abstract class OrderItem : DependencyObject
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public IEntity Source { get; private set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public string Name { get; private set; }

        protected OrderItem(IEntity source, string name)
        {
            Source = source;
            Name = name;
        }
        protected OrderItem(IOrderCompoundItem source)
        {
            Source = source.PrimaryComponent;
            var name = source.PrimaryComponent.Product.Name;
            if (source.Size != null)
                name += $" {source.Size.Name}";
            Name = name;
        }
    }
}
