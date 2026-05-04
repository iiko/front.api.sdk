using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Resto.Front.Api.Data.Assortment;
using System.Reactive.Disposables;

namespace Resto.Front.Api.SamplePlugin.DiagnosticMessagesTester
{
    internal sealed class StopListChangeNotifier : IDisposable
    {
        private static readonly IEqualityComparer<KeyValuePair<ProductAndSize, (decimal amount, DateTime?)>> RemainingsComparer = new ProductsComparer();
        private readonly CompositeDisposable subscriptions = new();
        private int stopListRevision;

        public StopListChangeNotifier()
        {
            subscriptions.Add(PluginContext.Notifications.StopListProductsRemainingAmountsChanged.Subscribe(OnStopListChanged));

            var remainingAmounts = PluginContext.Operations.GetStopListProductsRemainingAmounts();
            // ReSharper disable AccessToModifiedClosure
            subscriptions.Add(PluginContext.Notifications.StopListProductsRemainingAmountsChanged
                .Select(_ => PluginContext.Operations.GetStopListProductsRemainingAmounts())
                .Select(currentAmounts => new
                {
                    AddedProducts = currentAmounts
                         .Except(remainingAmounts, RemainingsComparer)
                         .ToArray(),
                    ChangedProducts = currentAmounts
                         .Intersect(remainingAmounts, RemainingsComparer)
                         .Select(x => new { Product = x.Key, AmountDiff = x.Value.amount - remainingAmounts[x.Key].amount })
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
                .Subscribe(message => PluginContext.Operations.AddNotificationMessage(message.ToString(), "SamplePlugin", TimeSpan.FromSeconds(3))));
            // ReSharper restore AccessToModifiedClosure
        }

        private void OnStopListChanged(VoidValue value)
        {
            var result = PluginContext.Operations.GetChangedStopListItems(stopListRevision);
            stopListRevision = result.RevisionTo;
            var sb = new StringBuilder();
            sb.AppendLine("Stop List changed:");
            sb.AppendLine($"Revision: {result.RevisionTo}");
            if (result.CurrentStopListItems.Count > 0)
            {
                sb.AppendLine();
                var currentProducts = string.Join(Environment.NewLine,
                    result.CurrentStopListItems.Select(k => $"{k.Key.Product.Name} ({k.Key.ProductSize?.Name ?? "No size"}), amount: {k.Value.Amount}, rev: {k.Value.Revision}"));
                sb.AppendLine($"Current list:{Environment.NewLine}{currentProducts}");
            }

            if (result.DeletedStopListItems.Count > 0)
            {
                sb.AppendLine();
                var deletedProducts = string.Join(Environment.NewLine,
                    result.DeletedStopListItems.Select(k => $"{k.Key.Product.Name} ({k.Key.ProductSize?.Name ?? "No size"}), amount: {k.Value.Amount}, rev: {k.Value.Revision}"));
                sb.AppendLine($"Deleted list:{Environment.NewLine}{deletedProducts}");
            }
            
            PluginContext.Log.Info(sb.ToString());
        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }

        private sealed class ProductsComparer : IEqualityComparer<KeyValuePair<ProductAndSize, (decimal, DateTime?)>>
        {
            public bool Equals(KeyValuePair<ProductAndSize, (decimal, DateTime?)> x, KeyValuePair<ProductAndSize, (decimal, DateTime?)> y)
            {
                return Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<ProductAndSize, (decimal, DateTime?)> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}
