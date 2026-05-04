using System.ComponentModel;
using System.Diagnostics;

using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.DataTransferObjects;
using Resto.Front.Api.Data.PreliminaryOrders;

namespace Resto.Front.Api.SamplePlugin.PreliminaryOrders
{
    internal sealed class ModifierItemModel : INotifyPropertyChanged
    {
        #region Fields
        private int amount;
        private IProduct dish;

        private ProductItemModel parent;
        #endregion

        #region Props
        public ProductItemModel Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public IProduct Dish
        {
            get { return dish; }
            set
            {
                if (dish == value)
                    return;

                dish = value;
                NotifyChanged("DishName");
            }
        }

        public string DishName
        {
            get { return Dish == null ? string.Empty : "    " + Dish.Name; }
        }

        public int Amount
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

        public bool IsChildModifier { get; set; }
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
        internal void Update([NotNull] IPreliminaryOrderModifierItem modifierItem)
        {
            Debug.Assert(modifierItem != null);

            Dish = modifierItem.Dish;
            Amount = modifierItem.Amount;
            IsChildModifier = modifierItem.IsChildModifier;
        }

        internal ModifierItemModel CreateCopy()
        {
            return new ModifierItemModel
                   {
                       Dish = Dish,
                       Amount = Amount,
                       IsChildModifier = IsChildModifier
                   };
        }

        internal PreliminaryOrderItemModifier CreateDtoItem()
        {
            return new PreliminaryOrderItemModifier
                   {
                       Dish = Dish,
                       Amount = Amount,
                       IsChildModifier = IsChildModifier
                   };
        }
        #endregion
    }
}