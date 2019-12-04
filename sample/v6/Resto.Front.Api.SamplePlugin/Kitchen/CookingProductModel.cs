using System;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    internal sealed class CookingProductModel
    {
        public CookingProductModel([NotNull] string name, decimal amount)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Amount = amount;
        }

        public string Name { get; }

        public decimal Amount { get; }
    }
}