using System;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Exceptions;

namespace Resto.Front.Api.SamplePlugin
{
    internal static class OperationServiceExtensions
    {
        private const string Pin = "12344321";

        [NotNull]
        public static ICredentials GetCredentials([NotNull] this IOperationService operationService)
        {
            if (operationService == null)
                throw new ArgumentNullException(nameof(operationService));

            try
            {
                return operationService.AuthenticateByPin(Pin);
            }
            catch (AuthenticationException)
            {
                PluginContext.Log.Warn("Cannot authenticate. Check pin for plugin user.");
                throw;
            }
        }

        [NotNull]
        public static ISubmittedEntities SubmitChanges([NotNull] this IOperationService os, [NotNull] IEditSession session)
        {
            if (os == null)
                throw new ArgumentNullException(nameof(os));
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            return os.SubmitChanges(os.GetCredentials(), session);
        }
    }
}
