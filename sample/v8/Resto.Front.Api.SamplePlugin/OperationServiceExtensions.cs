using System;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Editors;

namespace Resto.Front.Api.SamplePlugin
{
    internal static class OperationServiceExtensions
    {
        [NotNull]
        public static ISubmittedEntities SubmitChanges([NotNull] this IOperationService os, [NotNull] IEditSession session)
        {
            if (os == null)
                throw new ArgumentNullException(nameof(os));
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            return os.SubmitChanges(session, os.GetDefaultCredentials());
        }
    }
}
