using System.Windows;
using System.Windows.Controls;
using Resto.Front.Api.Data.View;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed class SectionSchemaItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var rsob = (RestaurantSectionObject)item;
            if (rsob is RestaurantSectionMark)
                return TextTemplate;
            if (rsob is RestaurantSectionImage)
                return ImageTemplate;
            if (rsob is RestaurantSectionTable)
                return TableTemplate;
            if (rsob is RestaurantSectionRectangle)
                return RectangleTemplate;
            if (rsob is RestaurantSectionEllipse)
                return EllipseTemplate;

            return null;
        }

        public DataTemplate RectangleTemplate { get; set; }
        public DataTemplate EllipseTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate TableTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }
    }
}