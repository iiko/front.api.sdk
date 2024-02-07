using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Resto.Front.Api.CustomerScreen.Helpers
{
    public sealed class DictionaryDataTemplateSelector : DataTemplateSelector
    {
        public static readonly Type Default = typeof(DictionaryDataTemplateSelector);

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            foreach (Type key in Options.Keys)
            {
                if (key.IsInstanceOfType(item))
                    return (DataTemplate)Options[key];
            }
            return Options.Contains(Default) ? (DataTemplate)Options[Default] : null;
        }

        public IDictionary Options { get; set; }
    }
}
