using Resto.Front.Api.SamplePlugin.WpfHelpers;

namespace Resto.Front.Api.SamplePlugin.Modifiers
{
    internal abstract class OrderItemViewModel : ViewModelBase
    {
        public virtual string DisplayText { get; set; }
        public virtual decimal Price { get; set; }
        public virtual decimal Amount { get; set; }
    }
}
