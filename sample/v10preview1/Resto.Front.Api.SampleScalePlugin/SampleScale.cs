using System;
using System.Collections.Generic;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Device.Results;
using Resto.Front.Api.Data.Device.Settings;
using Resto.Front.Api.Data.Device.Tasks;
using Resto.Front.Api.Devices;

namespace Resto.Front.Api.SampleScalePlugin
{
    internal sealed class SampleScale : IScale
    {
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
            PluginContext.Log.Info($"SampleScale: '{DeviceName} ({deviceId})' GetDeviceInfo.");
            return new DeviceInfo
            {
                State = state,
                Comment = "Работает",
                Settings = settings
            };
        }

        public void Start()
        {
            PluginContext.Log.Info($"SampleScale: '{DeviceName} ({deviceId})' was started");

            state = State.Running;
        }

        public void Stop()
        {
            PluginContext.Log.Info($"SampleScale: '{DeviceName} ({deviceId})' was stopped");

            state = State.Stopped;
        }

        public void Setup(DeviceSettings newSettings)
        {
            settings = newSettings;

            PluginContext.Log.Info($"SampleScale: '{DeviceName} ({deviceId})' was set up");
        }

        public void RemoveDevice()
        {
            PluginContext.Log.Info($"SampleScale: '{DeviceName} ({deviceId})' was removed");
        }

        public ScaleWeightResult MeasureWeight()
        {
            PluginContext.Log.Info($"SampleScale: '{DeviceName} ({deviceId})' MeasureWeight.");
            return new ScaleWeightResult(1.2m);
        }

        public ClearPluResult ClearPlu()
        {
            PluginContext.Log.Info($"SampleScale: '{DeviceName} ({deviceId})' ClearPlu.");

            return new ClearPluResult(true);
        }

        public SendPluResult SendPlu(SendPluTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var items = task.Items ?? new List<SendPluItem>();
            PluginContext.Log.Info(
                $"SampleScale: '{DeviceName} ({deviceId})' SendPlu was requested. Items={items.Count}, ResetBuffer={task.ResetBuffer}, StartUpload={task.StartUpload}");

            var results = new List<SendPluItemResult>(items.Count);
            foreach (var item in items)
            {
                PluginContext.Log.Info(
                    $"SampleScale: PLU item id={item.PluItemId}, dept={item.Department}, quickNo={item.QuickNo}, itemNo={item.ItemNo}, name='{item.Name}', " +
                    $"price={item.Price}, tare={item.WeightTare}, expiration={item.ExpirationDate}, description='{item.Description}', foodValue='{item.FoodValue}'");

                if (item.NutritionValue != null)
                {
                    PluginContext.Log.Info(
                        $"SampleScale: NutritionValue for {item.PluItemId}: calories={item.NutritionValue.Calories}, protein={item.NutritionValue.Protein}, " +
                        $"fat={item.NutritionValue.Fat}, carbohydrate={item.NutritionValue.Carbohydrate}");
                }
                else
                {
                    PluginContext.Log.Info($"SampleScale: NutritionValue for {item.PluItemId}: <null>");
                }

                results.Add(new SendPluItemResult(item.PluItemId, 0, "OK"));
            }

            return new SendPluResult(true, results);
        }
    }
}
