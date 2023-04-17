using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    public class TypeStyleSelector : StyleSelector
    {
        public static readonly Type Default = typeof(TypeStyleSelector);

        public override Style SelectStyle(object item, DependencyObject container)
        {
            foreach (Type key in Options.Keys)
            {
                if (key.IsInstanceOfType(item)) { return (Style)Options[key]; }
            }
            return Options.Contains(Default) ? (Style)Options[Default] : null;
        }

        public IDictionary Options { get; set; }
    }
}
