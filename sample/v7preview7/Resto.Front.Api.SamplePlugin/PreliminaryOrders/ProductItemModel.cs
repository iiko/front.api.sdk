using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.DataTransferObjects;
using Resto.Front.Api.Data.PreliminaryOrders;

namespace Resto.Front.Api.SamplePlugin.PreliminaryOrders
{
    internal sealed class ProductItemModel : INotifyPropertyChanged
    {
        #region Fields
        private decimal amount;
        private readonly IProduct dish;
        private IProductSize productSize;
        private readonly ObservableCollection<ModifierItemModel> modifiers = new ObservableCollection<ModifierItemModel>();
        private readonly ObservableCollection<IProductSize> availableSizes;

        private OrderModel parent;
        #endregion

        #region Ctors
        public ProductItemModel(IProduct dish, decimal amount, [CanBeNull] IProductSize productSize)
        {
            this.dish = dish;
            Amount = amount;
            ProductSize = productSize;

            availableSizes = new ObservableCollection<IProductSize>(dish.Scale != null
                ? PluginContext.Operations.GetProductScaleSizes(dish.Scale).Except(PluginContext.Operations.GetDisabledSizesByProduct(dish))
                : Enumerable.Empty<IProductSize>());
        }

        public ProductItemModel([NotNull] IPreliminaryOrderProductItem productItem)
            : this(productItem.Dish, productItem.Amount, productItem.ProductSize)
        {
            Debug.Assert(productItem != null);

            modifiers.Clear();
            foreach (var modifier in productItem.Modifiers)
            {
                var modifierItemModel = new ModifierItemModel();
                modifierItemModel.Update(modifier);
                modifierItemModel.Parent = this;
                modifiers.Add(modifierItemModel);
            }
        }
        #endregion

        #region Props
        public OrderModel Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public IProduct Dish
        {
            get { return dish; }
        }

        public string DishName
        {
            get { return Dish == null ? string.Empty : Dish.Name; }
        }

        public decimal Amount
        {
            get { return amount; }
            set
            {
                if (amount == value)
                    return;

                amount = value;
                NotifyChanged("Amount");
            }
        }

        public IProductSize ProductSize
        {
            get { return productSize; }
            set
            {
                if (productSize == value)
                    return;

                productSize = value;
                NotifyChanged("ProductSize");
            }
        }

        public IEnumerable<ModifierItemModel> Children
        {
            get { return modifiers; }
        }

        public IEnumerable<IProductSize> AvailableSizes
        {
            get { return availableSizes; }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void NotifyChanged(string propertyName)
        {
            var temp = PropertyChanged;
            if (temp != null)
                temp(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Methods
        internal ProductItemModel CreateCopy()
        {
            var copy = new ProductItemModel(Dish, Amount, ProductSize);

            foreach (var modifierCopy in modifiers.Select(modifier => modifier.CreateCopy()))
            {
                modifierCopy.Parent = copy;
                copy.modifiers.Add(modifierCopy);
            }

            return copy;
        }

        internal PreliminaryOrderItemProduct CreateDtoItem()
        {
            return new PreliminaryOrderItemProduct
                   {
                       Dish = Dish,
                       Amount = Amount,
                       ProductSize = ProductSize,
                       Modifiers = modifiers.Select(modifier => modifier.CreateDtoItem()).ToList()
                   };
        }

        internal void AddModifierItem(ModifierItemModel modifierItem)
        {
            modifierItem.Parent = this;
            modifiers.Add(modifierItem);

            if (parent != null)
                parent.RebuildItems();
        }

        internal void RemoveModifierItem(ModifierItemModel modifierItem)
        {
            modifierItem.Parent = null;
            modifiers.Remove(modifierItem);

            if (parent != null)
                parent.RebuildItems();
        }
        #endregion
    }
}