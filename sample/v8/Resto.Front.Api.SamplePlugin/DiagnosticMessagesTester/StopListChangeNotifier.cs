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
        private static readonly IEqualityComparer<KeyValuePair<ProductAndSize, decimal>> RemainingsComparer = new ProductsComparer();
        private readonly IDisposable subscription;

        public StopListChangeNotifier()
        {
            var remainingAmounts = PluginContext.Operations.GetStopListProductsRemainingAmounts();
            // ReSharper disable AccessToModifiedClosure
            subscription = PluginContext.Notifications.StopListProductsRemainingAmountsChanged
                .Select(_ => PluginContext.Operations.GetStopListProductsRemainingAmounts())
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
                         changes.AddedProducts.ForEach(x => message.AppendFormat(" {0} with size {1} ({2});",
                             x.Key.Product.Name, x.Key.ProductSize?.Name ?? "NULL", x.Value));
                         message.AppendLine();
                     }
                     if (changes.ChangedProducts.Length > 0)
                     {
                         message.Append("Changed:");
                         changes.ChangedProducts.ForEach(x => message.AppendFormat(" {0} {1}{2};", x.Product.Product.Name, x.AmountDiff > 0 ? "+" : string.Empty, x.AmountDiff));
                         message.AppendLine();
                     }
                     if (changes.DeletedProducts.Length > 0)
                     {
                         message.Append("Deleted:");
                         changes.DeletedProducts.ForEach(x => message.AppendFormat(" {0};", x.Key.Product.Name));
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

        private sealed class ProductsComparer : IEqualityComparer<KeyValuePair<ProductAndSize, decimal>>
        {
            public bool Equals(KeyValuePair<ProductAndSize, decimal> x, KeyValuePair<ProductAndSize, decimal> y)
            {
                return Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<ProductAndSize, decimal> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}
