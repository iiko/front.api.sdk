using System;
using Resto.Front.Api.V5.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin.Kitchen
{
    internal sealed class CookingProductModel
    {
        public CookingProductModel([NotNull] string name, decimal amount)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
            this.amount = amount;
        }

        private readonly string name;
        private readonly decimal amount;

        public string Name
        {
            get { return name; }
        }

        public decimal Amount
        {
            get { return amount; }
        }
    }
}