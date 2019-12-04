using System;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Device.Results;
using Resto.Front.Api.Data.Device.Settings;
using Resto.Front.Api.Devices;

namespace Resto.Front.Api.SampleScalePlugin
{
    internal sealed class SampleScale : MarshalByRefObject, IScale
    {
        // Note http://msdn.microsoft.com/en-us/library/23bk23zc(v=vs.100).aspx
        public override object InitializeLifetimeService() { return null; }

        private readonly Guid deviceId;
        [NotNull]
        private DeviceSettings settings;
        private const string DeviceFriendlyName = "Пример весов Штрих-Принт";
        private State state = State.Stopped;

        public SampleScale(Guid deviceId, DeviceSettings settings)
        {
            this.deviceId = deviceId;
            this.settings = settings;

            if (settings.Autorun)
                Start();
        }

        public Guid DeviceId => deviceId;

        public string DeviceName => DeviceFriendlyName;

        public DeviceInfo GetDeviceInfo()
        {
            return new DeviceInfo
            {
                State = state,
                Comment = "Работает",
                Settings = settings
            };
        }

        public void Start()
        {
            PluginContext.Log.InfoFormat("SampleScale: '{0} ({1})' was started", DeviceName, deviceId);

            state = State.Running;
        }

        public void Stop()
        {
            PluginContext.Log.InfoFormat("SampleScale: '{0} ({1})' was stopped", DeviceName, deviceId);

            state = State.Stopped;
        }

        public void Setup([NotNull] DeviceSettings newSettings)
        {
            settings = newSettings;

            PluginContext.Log.InfoFormat("SampleScale: '{0} ({1})' was set up", DeviceName, deviceId);
        }

        public void RemoveDevice()
        {
            PluginContext.Log.InfoFormat("SampleScale: '{0} ({1})' was removed", DeviceName, deviceId);
        }

        public ScaleWeightResult MeasureWeight()
        {
            return new ScaleWeightResult(1.2m);
        }
    }
}
