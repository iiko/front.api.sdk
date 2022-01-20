using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    public static class ChooseItemDialogHelper
    {
        public static bool ShowDialog<T>(IEnumerable<T> items, Func<T, string> itemNameConverter, out T selectedItem, string title = null, Window owner = null)
        {
            var listView = new ItemsControl();
            var selected = default(T);
            var isSelected = false;

            var window = new Window
            {
                Title = title ?? string.Empty,
                Content = new ScrollViewer
                {
                    Content = listView
                },
                SizeToContent = SizeToContent.WidthAndHeight,
                MaxWidth = 320,
                MaxHeight = 720,
                WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = owner
            };

            var clickHandler = new RoutedEventHandler((sender, args) =>
            {
                var button = (Button)sender;
                selected = (T)button.DataContext;
                isSelected = true;
                window.Close();
            });

            foreach (var item in items)
            {
                var button = new Button
                {
                    DataContext = item,
                    Content = itemNameConverter(item),
                    Margin = new Thickness(4),
                    Padding = new Thickness(4),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                button.Click += clickHandler;
                listView.Items.Add(button);
            }

            window.ShowDialog();

            selectedItem = selected;
            return isSelected;
        }
    }
}
