namespace Resto.Front.Api.SamplePlugin
{
    using static PluginContext;

    public sealed partial class SamplePlugin
    {
        private const int StartVersionedEntityRevision = 0;

        private void ResetMaxKnownVersionedEntityRevisionIfRequired()
        {
            var currentHostDatabaseId = Operations.GetHostDatabaseId();
            if (Config.HostDatabaseId == currentHostDatabaseId)
                return;

            Config.HostDatabaseId = currentHostDatabaseId;
            Config.MaxKnownVersionedEntityRevision = StartVersionedEntityRevision;
            Config.Save();
        }
    }
}