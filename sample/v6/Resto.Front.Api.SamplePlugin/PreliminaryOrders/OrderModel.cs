using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.DataTransferObjects;
using Resto.Front.Api.Data.PreliminaryOrders;

namespace Resto.Front.Api.SamplePlugin.PreliminaryOrders
{
    internal sealed class OrderModel : INotifyPropertyChanged
    {
        #region Fields
        private DateTime createTime;
        private string number;
        private string originName;
        private readonly ObservableCollection<ProductItemModel> products = new ObservableCollection<ProductItemModel>();
        private readonly ObservableCollection<object> items = new ObservableCollection<object>();
        #endregion

        #region Props
        public IPreliminaryOrder Source { get; private set; }

        public Guid Id { get; private set; }

        public DateTime CreateTime
        {
            get { return createTime; }
            set
            {
                if (createTime == value)
                    return;

                createTime = value;
                NotifyChanged("CreateTime");
            }
        }

        public string Number
        {
            get { return number; }
            set
            {
                if (string.Equals(number, value))
                    return;

                number = value;
                NotifyChanged("Number");
            }
        }

        public string OriginName
        {
            get { return originName; }
            set
            {
                if (string.Equals(originName, value))
                    return;

                originName = value;
                NotifyChanged("OriginName");
            }
        }

        public ObservableCollection<object> Items
        {
            get { return items; }
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
        internal void Update([NotNull] IPreliminaryOrder order)
        {
            Debug.Assert(order != null);

            Source = order;
            Id = order.Id;
            CreateTime = order.CreateTime;
            Number = order.Number;
            OriginName = order.OriginName;

            products.Clear();
            foreach (var product in order.Products)
            {
                products.Add(new ProductItemModel(product) { Parent = this });
            }

            RebuildItems();
        }

        internal List<PreliminaryOrderItemProduct> CreateProductDtoItems()
        {
            return products
                .Select(product => product.CreateDtoItem())
                .ToList();
        }

        internal OrderModel CreateCopy()
        {
            var copy = new OrderModel
                       {
                           Source = Source,
                           Id = Id,
                           CreateTime = CreateTime,
                           Number = Number,
                           OriginName = OriginName
                       };

            foreach (var productCopy in products.Select(product => product.CreateCopy()))
            {
                productCopy.Parent = copy;
                copy.products.Add(productCopy);
            }

            copy.RebuildItems();

            return copy;
        }

        internal void AddProductItem(ProductItemModel productItem)
        {
            productItem.Parent = this;
            products.Add(productItem);
            RebuildItems();
        }

        internal void RemoveProductItem(ProductItemModel productItem)
        {
            productItem.Parent = null;
            products.Remove(productItem);
            RebuildItems();
        }

        internal void RebuildItems()
        {
            items.Clear();

            foreach (var product in products)
            {
                items.Add(product);

                foreach (var modifier in product.Children)
                {
                    items.Add(modifier);
                }
            }
        }
        #endregion
    }
}