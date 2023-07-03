using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Resto.Front.Api.CustomerScreen.Helpers;

namespace Resto.Front.Api.CustomerScreen.Resources
{
    public sealed class CustomTypedContainerItemsControl : ItemsControl
    {
        private static readonly Type ContentPresenterType = typeof(ContentPresenter);
        private static readonly Type ContentControlType = typeof(ContentControl);

        public static readonly DependencyProperty ContainerTypeProperty = DependencyPropertyHelper<CustomTypedContainerItemsControl>.
            Register(o => o.ContainerType, typeof(ContentPresenter), ValidateContainerType);

        private static readonly DependencyPropertyKey ScrollViewerPropertyKey = DependencyPropertyHelper<CustomTypedContainerItemsControl>
            .RegisterReadOnly(o => o.ScrollViewer);
        public static readonly DependencyProperty ScrollViewerProperty = ScrollViewerPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ItemsHostPanelPropertyKey = DependencyPropertyHelper<CustomTypedContainerItemsControl>
            .RegisterReadOnly(o => o.ItemsHostPanel);
        public static readonly DependencyProperty ItemsHostPanelProperty = ItemsHostPanelPropertyKey.DependencyProperty;

        private static bool ValidateContainerType(object value)
        {
            var type = value as Type;
            if (type == null)
                return false;

            return type == ContentPresenterType
                || type == ContentControlType
                || type.IsSubclassOf(ContentControlType);
        }

        public Type ContainerType
        {
            get { return (Type)GetValue(ContainerTypeProperty); }
            set { SetValue(ContainerTypeProperty, value); }
        }

        public Panel ItemsHostPanel
        {
            get { return (Panel)GetValue(ItemsHostPanelProperty); }
            set
            {
                SetValue(ItemsHostPanelPropertyKey, value);
                IsScrollViewerValid = false;
            }
        }

        public ScrollViewer ScrollViewer
        {
            get
            {
                EnsureScrollViewerValid();
                return (ScrollViewer)GetValue(ScrollViewerProperty);
            }
        }

        private bool IsScrollViewerValid { get; set; }

        public CustomTypedContainerItemsControl()
        {
            IsScrollViewerValid = false;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return (DependencyObject)Activator.CreateInstance(ContainerType);
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);
            InvalidateScrollViewer();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var desiredSize = base.MeasureOverride(constraint);
            EnsureScrollViewerValid();
            return desiredSize;
        }

        private void InvalidateScrollViewer()
        {
            ClearValue(ScrollViewerPropertyKey);
            ClearValue(ItemsHostPanelPropertyKey);
            IsScrollViewerValid = false;
        }

        private void EnsureScrollViewerValid()
        {
            if (!IsScrollViewerValid)
            {
                SetValue(ScrollViewerPropertyKey, GetScrollViewer());
                IsScrollViewerValid = true;
            }
        }

        private ScrollViewer GetScrollViewer()
        {
            var itemsHost = ItemsHostPanel;
            if (itemsHost == null)
                return null;

            for (DependencyObject element = itemsHost; element != this && element != null; element = VisualTreeHelper.GetParent(element))
            {
                // Обходим ошибку в .NET 4.0 x64. Закомментированный код не работает, «as» возвращает null для экземпляра ScrollViewer.
                //                var viewer = element as ScrollViewer;
                //                if (viewer != null)
                //                    return viewer;

                if (element.GetType() == typeof(ScrollViewer))
                    return (ScrollViewer)element;
            }
            return null;
        }
    }
}
