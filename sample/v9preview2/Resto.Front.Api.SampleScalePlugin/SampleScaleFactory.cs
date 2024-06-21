﻿using System;
using System.Collections.Generic;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Device.Settings;
using Resto.Front.Api.Devices;

namespace Resto.Front.Api.SampleScalePlugin
{
    internal sealed class SampleScaleFactory : IScaleFactory
    {
        [NotNull]
        private const string PluginFactoryCode = "{E81151C3-F1FB-40F9-AA09-A6B5CD513AF0}";

        [NotNull]
        private const string ScaleName = "Пример плагина весов Штрих-Принт";

        public SampleScaleFactory()
        {
            DefaultDeviceSettings = InitDefaultDeviceSettings();
        }

        public string FactoryCode => PluginFactoryCode;
        public string Description => ScaleName;

        [NotNull]
        public DeviceSettings DefaultDeviceSettings { get; }

        public IScale Create(Guid deviceId, DeviceSettings settings)
        {
            var scale = new SampleScale(deviceId, settings);

            return scale;
        }

        private DeviceSettings InitDefaultDeviceSettings()
        {
            return new DeviceSettings
            {
                FactoryCode = PluginFactoryCode,
                Description = Description,
                Settings = new List<DeviceSetting>
                {
                    new DeviceNumberSetting
                    {
                        Name = "Int Setting",
                        Value = 1,
                        Label = "Настройка целого значения",
                        MaxValue = 999,
                        MinValue = 1,
                        SettingKind = DeviceNumberSettingKind.Integer
                    },
                    new DeviceStringSetting
                    {
                        Name = "String setting",
                        Label = "Настройка для ввода строки",
                        Value = "А",
                        MaxLength = 255
                    }
                }
            };
        }

    }
}
