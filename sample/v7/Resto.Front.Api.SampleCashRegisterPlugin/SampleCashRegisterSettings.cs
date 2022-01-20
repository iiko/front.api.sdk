using System.Collections.Generic;
using System.Linq;
using Resto.Front.Api.Data.Device.Settings;

namespace Resto.Front.Api.SampleCashRegisterPlugin
{
    /// <summary>
    /// Здесь необходимо описать все доступные настройки для данного устройства
    /// </summary>
    public class SampleCashRegisterSettings
    {
        private readonly CashRegisterSettings deviceSettings;

        public SampleCashRegisterSettings(CashRegisterSettings settings)
        {
            deviceSettings = settings;
        }

        private T GetSetting<T>(string name) where T : DeviceSetting
        {
            return (T)deviceSettings.Settings.FirstOrDefault(setting => setting.Name == name);
        }

        /// Пример добавления настроек
        /// Тип настройки, унаследованный от <seealso cref="DeviceSetting"/>
        /// Числовой <seealso cref="DeviceNumberSetting"/>, текстовый <seealso cref="DeviceStringSetting"/>, 
        /// Флаг <seealso cref="DeviceBooleanSetting"/>, перечисление <seealso cref="DeviceCustomEnumSetting"/>
        public DeviceNumberSetting NumberSettingExample => GetSetting<DeviceNumberSetting>("NumberSettingExample");

        /// Для каждой настройки надо укажать стандартные значения и ограничения
        /// Для числовых настроек: <para />
        /// Обязательно имя - <seealso cref="DeviceNumberSetting.Name"/>
        /// Стандартное значение - <seealso cref="DeviceNumberSetting.Value"/>
        /// Описание - <seealso cref="DeviceNumberSetting.Label"/>
        /// Максимальное значение - <seealso cref="DeviceNumberSetting.MaxValue"/>
        /// Минимальное значение - <seealso cref="DeviceNumberSetting.MinValue"/>
        /// Числовой тип - <seealso cref="DeviceNumberSetting.SettingKind"/>
        public static readonly DeviceNumberSetting DefaultNumberSettingExample =
            new DeviceNumberSetting
            {
                Name = "NumberSettingExample",
                Value = 1,
                Label = "Пример числовой настройки",
                MaxValue = 999,
                MinValue = 1,
                SettingKind = DeviceNumberSettingKind.Integer
            };

        public DeviceStringSetting StringSettingExample => GetSetting<DeviceStringSetting>("StringSettingExample");

        /// Для строковых настроек: <para />
        /// Обязательно имя - <seealso cref="DeviceStringSetting.Name"/>
        /// Стандартное значение - <seealso cref="DeviceStringSetting.Value"/>
        /// Описание - <seealso cref="DeviceStringSetting.Label"/>
        /// Максимальная длина - <seealso cref="DeviceStringSetting.MaxLength"/>
        public static readonly DeviceStringSetting DefaultStringSettingExample =
            new DeviceStringSetting
            {
                Name = "StringSettingExample",
                Label = "Пример строковой настройки",
                Value = "1.0",
                MaxLength = 255,
            };

        public DeviceBooleanSetting BooleanSettingExample => GetSetting<DeviceBooleanSetting>("BooleanSettingExample");

        /// Для флаговых настроек: <para />
        /// Обязательно имя - <seealso cref="DeviceBooleanSetting.Name"/>
        /// Стандартное значение - <seealso cref="DeviceBooleanSetting.Value"/>
        /// Описание - <seealso cref="DeviceBooleanSetting.Label"/>
        public static readonly DeviceBooleanSetting DefaultPrintItemsOnCheque =
            new DeviceBooleanSetting
            {
                Name = "BooleanSettingExample",
                Value = true,
                Label = "Пример флаговой настройки",
            };

        public DeviceCustomEnumSetting ListSettingExample => GetSetting<DeviceCustomEnumSetting>("ListSettingExample");

        /// Для перечислений: <para />
        /// Имя - <seealso cref="DeviceCustomEnumSetting.Name"/>
        /// Описание - <seealso cref="DeviceCustomEnumSetting.Label"/>
        /// Тип отображения - <seealso cref="DeviceCustomEnumSetting.IsList"/>
        /// Варианты выбора - <seealso cref="DeviceCustomEnumSetting.Values"/>, <para />
        /// Для каждого варианта: <para />
        /// Имя - <seealso cref="DeviceCustomEnumSettingValue.Name"/>
        /// Стандартное значение (выбрано или нет) - <seealso cref="DeviceCustomEnumSettingValue.IsDefault"/>
        /// Описание - <seealso cref="DeviceCustomEnumSettingValue.Label"/>
        public static readonly DeviceCustomEnumSetting DefaultListSettingExample =
            new DeviceCustomEnumSetting()
            {
                Name = "ListSettingExample",
                Label = "Пример настройки перечисления",
                IsList = true,
                Values = new List<DeviceCustomEnumSettingValue>
                {
                    new DeviceCustomEnumSettingValue()
                    {
                        Name = "ListElement1",
                        IsDefault = true,
                        Label = "Элемент списка 1"
                    },
                    new DeviceCustomEnumSettingValue()
                    {
                        Name = "ListElement2",
                        IsDefault = false,
                        Label = "Элемент списка 2"
                    }
                }
            };
    }
}