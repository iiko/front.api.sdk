using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Resto.Front.Api.V5;
using Resto.Front.Api.V5.Data.Cheques;
using Resto.Front.Api.V5.Extensions;
using Resto.Front.Api.SamplePlugin.Properties;
using Resto.Front.Api.V5.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin
{
    /// <summary>
    /// Расширяет разметку пречека, дописывая номер чека по внутреннему счётчику.
    /// </summary>
    internal sealed class BillChequeExtender : IDisposable
    {
        [NotNull]
        private readonly IDisposable subscription;
        [NotNull]
        private readonly Dictionary<Guid, int> extendedCheques = new Dictionary<Guid, int>();
        private int prevChequeNumber;

        internal BillChequeExtender()
        {
            subscription = PluginContext.Notifications.SubscribeOnBillChequePrinting(AddBillChequeExtensions);
        }

        /// <summary>
        /// Добавить дополнительные поля к пречеку.
        /// </summary>        
        [NotNull]
        private BillCheque AddBillChequeExtensions(Guid orderId)
        {
            int chequeNumber;
            if (extendedCheques.TryGetValue(orderId, out chequeNumber))
                return AddDuplicatedChequeExtensions(chequeNumber);

            chequeNumber = prevChequeNumber++;
            extendedCheques.Add(orderId, chequeNumber);
            return AddUniqueChequeExtensions(chequeNumber);
        }

        [NotNull]
        private static BillCheque AddUniqueChequeExtensions(int chequeNumber)
        {
            var billCheque = new BillCheque
            {
                BeforeHeader = new XElement(Tags.Left, Resources.WelcomeText),
                BeforeFooter = new XElement(Tags.Pair, new XAttribute(Tags.Left, Resources.ChequeNumber), new XAttribute(Tags.Right, chequeNumber.ToString())),
                AfterFooter = new XElement(Tags.Center, Resources.ValedictoryText)
            };
            if (chequeNumber % 10 == 0)
                billCheque.AfterHeader = new XElement(Tags.Table,
                    new XElement(Tags.Columns,
                        new XElement(Tags.Column, new XAttribute(Attributes.AutoWidth, "")),
                        new XElement(Tags.Column, new XAttribute(Attributes.Align, "right"))),
                    new XElement(Tags.Cells,
                        new XElement(Tags.Cell, Resources.RoundNumberCongratulation, new XAttribute(Attributes.ColumnSpan, 2)),
                        new XElement(Tags.TextCell, Resources.RoundChequeNumber),
                        new XElement(Tags.TextCell, chequeNumber.ToString())));
            return billCheque;
        }

        [NotNull]
        private static BillCheque AddDuplicatedChequeExtensions(int chequeNumber)
        {
            return new BillCheque
            {
                BeforeHeader = new XElement(Tags.Center, Resources.DuplicatedCheque),
                BeforeFooter = new XElement(Tags.Pair, new XAttribute(Tags.Left, Resources.ChequeNumber), new XAttribute(Tags.Right, chequeNumber.ToString())),
            };
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}
