using System;
using System.Globalization;
using System.Windows.Data;
using Resto.Front.Api.SamplePlugin.Restaurant;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal sealed class ProductToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case ProductModel model when model.Product.HasMenuImage:
                    return ImageBytesToBitmapConverter.CreateBitmap(model.Product.GetProductMenuImage());
                case ProductGroupModel model when model.ProductGroup.HasMenuImage:
                    return ImageBytesToBitmapConverter.CreateBitmap(model.ProductGroup.GetProductGroupMenuImage());
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
