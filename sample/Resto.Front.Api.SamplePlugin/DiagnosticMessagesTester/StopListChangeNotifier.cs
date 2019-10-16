using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Resto.Front.Api.Data.Assortment;

namespace Resto.Front.Api.SamplePlugin.DiagnosticMessagesTester
{
    internal sealed class StopListChangeNotifier : IDisposable
    {
        private static readonly IEqualityComparer<KeyValuePair<IProduct, decimal>> RemainingsComparer = new ProductsComparer();
        private readonly IDisposable subscription;

        public StopListChangeNotifier()
        {
            var remainingAmounts = PluginContext.Operations.GetProductsRemainingAmounts();
            // ReSharper disable AccessToModifiedClosure
            subscription = PluginContext.Notifications.ProductsRemainingAmountsChanged
                .Select(_ => PluginContext.Operations.GetProductsRemainingAmounts())
                .Select(currentAmounts => new
                 {
                     AddedProducts = currentAmounts
                         .Except(remainingAmounts, RemainingsComparer)
                         .ToArray(),
                     ChangedProducts = currentAmounts
                         .Intersect(remainingAmounts, RemainingsComparer)
                         .Select(x => new { Product = x.Key, AmountDiff = x.Value - remainingAmounts[x.Key] })
                         .Where(x => x.AmountDiff != 0m)
                         .ToArray(),
                     DeletedProducts = remainingAmounts
                         .Except(currentAmounts, RemainingsComparer)
                         .ToArray(),
                     RemainingAmounts = currentAmounts
                 })
                .Do(changes => remainingAmounts = changes.RemainingAmounts)
                .Select(changes =>
                 {
                     var message = new StringBuilder();
                     if (changes.AddedProducts.Length > 0)
                     {
                         message.Append("Added:");
                         changes.AddedProducts.ForEach(x => message.AppendFormat(" {0} ({1});", x.Key.Name, x.Value));
                         message.AppendLine();
                     }
                     if (changes.ChangedProducts.Length > 0)
                     {
                         message.Append("Changed:");
                         changes.ChangedProducts.ForEach(x => message.AppendFormat(" {0} {1}{2};", x.Product.Name, x.AmountDiff > 0 ? "+" : string.Empty, x.AmountDiff));
                         message.AppendLine();
                     }
                     if (changes.DeletedProducts.Length > 0)
                     {
                         message.Append("Deleted:");
                         changes.DeletedProducts.ForEach(x => message.AppendFormat(" {0};", x.Key.Name));
                     }
                     return message;
                 })
                .Where(message => message.Length > 0)
                .Subscribe(message => PluginContext.Operations.AddNotificationMessage(message.ToString(), "SamplePlugin", TimeSpan.FromSeconds(3)));
            // ReSharper restore AccessToModifiedClosure
        }

        public void Dispose()
        {
            subscription.Dispose();
        }

        private sealed class ProductsComparer : IEqualityComparer<KeyValuePair<IProduct, decimal>>
        {
            public bool Equals(KeyValuePair<IProduct, decimal> x, KeyValuePair<IProduct, decimal> y)
            {
                return Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<IProduct, decimal> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}
