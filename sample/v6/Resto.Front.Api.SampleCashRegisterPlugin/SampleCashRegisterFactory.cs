using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Device.Results;
using Resto.Front.Api.Data.Device.Settings;
using Resto.Front.Api.Devices;

namespace Resto.Front.Api.SampleCashRegisterPlugin
{
    internal sealed class SampleCashRegisterFactory : MarshalByRefObject, ICashRegisterFactory
    {
        // Note http://msdn.microsoft.com/en-us/library/23bk23zc(v=vs.100).aspx
        public override object InitializeLifetimeService() { return null; }

        [NotNull]
        private const string CashRegisterName = "Пример плагина внешнего фискального регистратора";

        public SampleCashRegisterFactory()
        {
            DefaultDeviceSettings = InitDefaultDeviceSettings();
        }

        [NotNull]
        public string CodeName => CashRegisterName;

        [NotNull]
        public DeviceSettings DefaultDeviceSettings { get; }

        //Инициализировать настройки фискального регистратора
        private CashRegisterSettings InitDefaultDeviceSettings()
        {
            return new CashRegisterSettings
            {
                Code = CodeName,
                Settings = new List<DeviceSetting>(
                    typeof(SampleCashRegisterSettings).GetFields(BindingFlags.Static | BindingFlags.Public).Select(info => (DeviceSetting)info.GetValue(null))),
                Font0Width = new DeviceNumberSetting
                {
                    Name = "Font0Width",
                    Value = 42,
                    Label = "Символов в строке",
                    MaxValue = 100,
                    MinValue = 10,
                    SettingKind = DeviceNumberSettingKind.Integer
                },
                OfdProtocolVersion = new DeviceCustomEnumSetting
                {
                    Name = "OfdProtocolVersion",
                    Label = "Версия ФФД",
                    Values = new List<DeviceCustomEnumSettingValue>
                    {
                        new DeviceCustomEnumSettingValue
                        {
                            Name = string.Empty,
                            IsDefault = true,
                            Label = "Без ФФД"
                        },
                        new DeviceCustomEnumSettingValue
                        {
                            Name = "1.0",
                            IsDefault = false,
                            Label = "1.0"
                        },
                        new DeviceCustomEnumSettingValue
                        {
                            Name = "1.05",
                            IsDefault = false,
                            Label = "1.05"
                        },
                        new DeviceCustomEnumSettingValue
                        {
                            Name = "1.1",
                            IsDefault = false,
                            Label = "1.1"
                        }
                    }
                },
                FiscalRegisterPaymentTypes = new List<FiscalRegisterPaymentType>
                {
                    //Заполнить таблицу типов оплат
                    new FiscalRegisterPaymentType
                    {
                        Id = "1",
                        Name = "Тип оплаты 1"
                    },
                    new FiscalRegisterPaymentType
                    {
                        Id = "2",
                        Name = "Тип оплаты 2"
                    },
                    new FiscalRegisterPaymentType
                    {
                        Id = "3",
                        Name = "Тип оплаты 3"
                    },
                },
                //Заполнить таблицу налоговых ставок
                FiscalRegisterTaxItems = new List<FiscalRegisterTaxItem>
                {
                    new FiscalRegisterTaxItem("1", false, true, 0, "НДС 0%"),
                    new FiscalRegisterTaxItem("2", false, true, 18, "НДС 20%")
                }
            };
        }

        public ICashRegister Create(Guid deviceId, [NotNull] CashRegisterSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var sampleCashRegister = new SampleCashRegister(deviceId, settings);

            return sampleCashRegister;
        }
    }
}
