using System;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.SamplePlugin.Config;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class ServiceFactory
    {
        public static ServiceFactory Instance { get; } = new ServiceFactory();

        private bool isInitialized;

        public void Init()
        {
            configService = new ConfigService();

            isInitialized = true;
        }

        private ConfigService configService;
        [NotNull]
        public ConfigService ConfigService
        {
            get
            {
                CheckServiceIsInitialized();
                return configService;
            }
        }

        private void CheckServiceIsInitialized()
        {
            if (!isInitialized)
                throw new InvalidOperationException("ServiceFactory is not initialized");
        }
    }
}
