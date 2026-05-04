using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Device.Results;
using Resto.Front.Api.Data.Device.Settings;
using Resto.Front.Api.Data.Device.Tasks;
using Resto.Front.Api.Data.Print;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Devices;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SampleCashRegisterPlugin
{
    internal sealed class SampleCashRegister : IFfdCashRegister, IAutosearchCashRegister
    {
        private readonly Guid deviceId;
        [NotNull]
        private CashRegisterSettings cashRegisterSettings;
        private const string DeviceFriendlyName = "Пример фискального регистратора";
        private State state = State.Stopped;

        public SampleCashRegister(Guid deviceId, CashRegisterSettings cashRegisterSettings)
        {
            this.deviceId = deviceId;
            this.cashRegisterSettings = cashRegisterSettings;
        }

        public Guid DeviceId => deviceId;

        public string DeviceName => DeviceFriendlyName;

        /// <summary>
        /// Проверить, что устройство запущено.
        /// Исключение <see cref="DeviceNotStartedException"/> необходимо
        /// для того, чтобы при включенной настройке "Автозапуск"
        /// попытка запуска устройства выполнялась автоматически.
        /// </summary>
        private void CheckStarted()
        {
            if (state != State.Running)
                throw new DeviceNotStartedException("Device not started");
        }

        /// <summary>
        /// Получить текущее состояние ФР
        /// </summary>
        /// <returns><see cref="DeviceInfo"/> 
        /// <see cref="DeviceInfo.State"/> Текстовая строка состояния<para />
        /// <see cref="DeviceInfo.Comment"/> Описание статуса устройства<para />
        /// </returns>
        public DeviceInfo GetDeviceInfo()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' GetDeviceInfo.", DeviceName, deviceId);
            return new DeviceInfo
            {
                State = state,
                Comment = "Работает",
                Settings = cashRegisterSettings
            };
        }

        /// <summary>
        /// Команда выполняет инициализацию драйвера, открытие порта, соединение с устройством и тестирование соединения
        /// </summary>
        public void Start()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' was started", DeviceName, deviceId);
            var paymentTypes = new List<FiscalRegisterPaymentType>
            {
                new() { Id = "1", Name = "Non-cash payment type 1" },
                new() { Id = "2", Name = "Non-cash payment type 2" },
                new() { Id = "3", Name = "Non-cash payment type 3" },
                new() { Id = "4", Name = "Non-cash payment type 4" },
                new() { Id = "5", Name = "Non-cash payment type 5" },
            };

            var taxItems = new List<FiscalRegisterTaxItem>
            {
                new ("1", true, false, 10.0m, "VAT 10%"),
                new ("2", true, false, 20.0m, "VAT 20%"),
                new ("3", true, false, 0.0m, "VAT 0%"),
                new ("4", false, false, 0.0m, "Exempted VAT"),
            };
            cashRegisterSettings.FiscalRegisterPaymentTypes = paymentTypes;
            cashRegisterSettings.FiscalRegisterTaxItems = taxItems;
            state = State.Running;
        }

        /// <summary>
        /// Команда выполняет остановку устройства, освобождение ресурсов, закрытие портов
        /// </summary>
        public void Stop()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' was stopped", DeviceName, deviceId);

            state = State.Stopped;
        }

        /// <summary>
        /// Команда для выполнения изменения настроек устройства.
        /// </summary>
        public void Setup([NotNull] DeviceSettings newSettings)
        {
            if (newSettings == null)
                throw new ArgumentNullException(nameof(newSettings));

            cashRegisterSettings = (CashRegisterSettings)newSettings;

            PluginContext.Log.InfoFormat("Device: '{0} ({1})' was setup", DeviceName, deviceId);
        }

        public void RemoveDevice()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' was removed", DeviceName, deviceId);
        }

        /// <summary>
        /// Операция печати чека включает в себя открытие фискального чека, 
        /// Добавление предметов расчёта, дополнительные реквизиты, округление копеек или скидки, закрытие чека.
        /// Для чека возврата поле <paramref name="chequeTask.IsRefund"/> имеет значение true 
        /// </summary>
        /// Объект <see cref="ChequeTask" />, который содерджит следующие данные: <para />
        /// <see cref="ChequeTask.TextAfterCheque"/> Текст, который должен быть напечатан до тела чека, может быть пустым<para />
        /// <see cref="ChequeTask.TableNumber"/> Номер стола заказа<para />
        /// <see cref="ChequeTask.TextBeforeCheque"/> Текст, который должен быть распечатан после тела чека, может быть пустым<para />
        /// <see cref="ChequeTask.Sales"/> Список предметов расчёта<para />
        /// <see cref="ChequeTask.RoundSum"/> Сумма округления копеек, скидка содержится в поле <see cref="ChequeTask.DiscountSum"/>, <para />
        /// <see cref="ChequeTask.OfdEmail"/> Адрес электронной почты покупателя, на который будет отправлен электронный чек, имеет более высокий приоритет, чем <see cref="ChequeTask.OfdPhoneNumber"/> ,<para />
        /// <see cref="ChequeTask.OfdPhoneNumber"/> Номер телефона покупателя, на который будет отправлен электронный чек<para />
        /// <see cref="ChequeTask.CashPayment"/> Сумма оплаты наличными<para />
        /// <see cref="ChequeTask.CardPayments"/> Список оплат, кроме наличных, список типов оплат должен быть синхронизированы с идентификаторами оплат на ФР посредством настроек в BackOffice<para />
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение.
        /// </returns>
        public CashRegisterResult DoCheque(ChequeTask chequeTask, IViewManager viewManager, IOperationDataContext operationDataContext, IOperationService operationService)
        {
            PluginContext.Log.InfoFormat("chequeTask.IsBuy: {0}", chequeTask.IsBuy);
            PluginContext.Log.InfoFormat("chequeTask.IsRefund: {0}", chequeTask.IsRefund);
            PluginContext.Log.InfoFormat("chequeTask.IsCancellation: {0}", chequeTask.IsCancellation);
            foreach (var sale in chequeTask.Sales)
                PluginContext.Log.InfoFormat("{0} x{1} {2}", sale.Name, sale.Amount, sale.Sum);

            CheckStarted();
            #region Пример чтения настроек
            var settings = new SampleCashRegisterSettings(cashRegisterSettings);
            //Получаем число
            var fontWidth = settings.NumberSettingExample.Value;
            //Получаем строку
            var ofdVersion = settings.StringSettingExample.Value;
            //Получаем флаг
            var printOrderNumber = settings.BooleanSettingExample.Value;
            //Получаем перечисление
            var fixPrepay = settings.ListSettingExample.Values.First(value => value.IsDefault).Name;
            #endregion Пример чтения настроек

            PluginContext.Log.InfoFormat("Device: '{0} ({1})' Cheque printed.", DeviceName, deviceId);

            //Пример передачи налогов фактически из плагина 
            List<CashRegisterVatData> vatTotalizers = new List<CashRegisterVatData>();
            foreach (ChequeSale sale in chequeTask.Sales)
            {
                var vatItem = vatTotalizers.FirstOrDefault(item => (item.TaxId == sale.TaxId) ||
                                                                   (item.IsTaxable == sale.IsTaxable &&
                                                                    item.TaxPercent == sale.Vat.GetValueOrDefault()));
                var rate = sale.Vat.GetValueOrDefault();

                if (vatItem == null)
                {
                    vatItem = new CashRegisterVatData(sale.TaxId, sale.IsTaxable, true,
                        sale.Vat.GetValueOrDefault(), $"vat {rate}%");
                    vatTotalizers.Add(vatItem);

                }

                //Тут можно прочитать фактическую сумму налоговой ставки из ФР
                vatItem.TaxAmount += decimal.Round(rate * sale.Sum.GetValueOrDefault() / (100 + rate), 2,
                    MidpointRounding.AwayFromZero);
                vatItem.HaveTaxAmount = true;
            }

            var result = GetCashRegisterData();
            result.ChequeVatTotalizers = vatTotalizers;
            return result;
        }

        /// <summary>
        /// Получить параметры ФР, эти параметры должны быть актуальны на момент получения
        /// </summary>
        /// <returns>
        /// Объект <see cref="CashRegisterResult" />, который содерджит следующие данные: <para />
        /// Сумма наличных в кассе <see cref="CashRegisterResult.CashSum" /><para />
        /// Общая сумма выручки за смену <see cref="CashRegisterResult.TotalIncomeSum" /><para />
        /// Номер смены <see cref="CashRegisterResult.Session" /><para />
        /// Серийный номер ФР <see cref="CashRegisterResult.SerialNumber" /><para />
        /// Номер документа <see cref="CashRegisterResult.DocumentNumber" /><para />
        /// Количество продаж <see cref="CashRegisterResult.SaleNumber" /><para />
        /// Текущая дата и время на ФР <see cref="CashRegisterResult.RtcDateTime" /><para />
        /// Номер заказа, не актуален для большинства ФР, можно передавать пустым <see cref="CashRegisterResult.BillNumber" /><para />
        /// Так же может содержать суммы регистров по всем налогам в <see cref="CashRegisterResult.MapVatResult" /><para />
        /// И суммы налогов за текущий чек в <see cref="CashRegisterResult.MapVatResultForOrder" /><para />
        /// </returns>
        public CashRegisterResult GetCashRegisterData()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' GetCashRegisterData.", DeviceName, deviceId);
            return new CashRegisterResult(null, null, null, "", null, null, null, "", "", "", null);
        }

        /// <summary>
        /// Вызвать печать Z-отчета (Закрытие смены)
        /// </summary>
        /// <param name="cafeSession">Закрываемая кассовая смена</param>
        /// <param name="zReportTask">Таск для печати Z-отчета</param>
        /// <param name="user">Пользователь проводящий печать</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoZReport(ICafeSession cafeSession, ZReportTask zReportTask, IUser user, IViewManager viewManager)
        {
            CheckStarted();

            PluginContext.Log.InfoFormat("Device: '{0} ({1})' Z-report printed.", DeviceName, deviceId);
            return GetCashRegisterData();
        }

        /// <summary>
        /// Проверить состояние устройства
        /// </summary>
        /// <param name="getCashRegisterStatusTask">Объект со списком запрашиваемых полей</param>
        /// <returns>Возвращает экземпляры <see cref="CashRegisterStatus"/>, с заполненными запрошенными полями, либо null, если значение не заполнено.
        /// </returns>
        public CashRegisterStatus GetCashRegisterStatus(GetCashRegisterStatusTask getCashRegisterStatusTask)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' GetCashRegisterStatus.", DeviceName, deviceId);
            var result = new CashRegisterStatus();
            if (getCashRegisterStatusTask.StatusFields.Contains(CashRegisterStatusField.RegistrationNumber))
                result.RegistrationNumber = "123123123123123123";
            if (getCashRegisterStatusTask.StatusFields.Contains(CashRegisterStatusField.SerialNumber))
                result.SerialNumber = "123123123123123124";
            if (getCashRegisterStatusTask.StatusFields.Contains(CashRegisterStatusField.FiscalModuleSerialNumber))
                result.FiscalModuleSerialNumber = "123123123123123125";
            if (getCashRegisterStatusTask.StatusFields.Contains(CashRegisterStatusField.FiscalModuleActivationDate))
                result.FiscalModuleActivationDate = DateTime.Today;
            if (getCashRegisterStatusTask.StatusFields.Contains(CashRegisterStatusField.FiscalModuleExpirationDate))
                result.FiscalModuleExpirationDate = DateTime.Today.AddMonths(2);
            if (getCashRegisterStatusTask.StatusFields.Contains(CashRegisterStatusField.FiscalModuleHealth))
                result.FiscalModuleWarnings = new List<FiscalModuleWarningInfo>{ new FiscalModuleWarningInfo { Code = FiscalModuleWarningCode.Memory99Percent, Message = "Test message" } };
            return result;
        }

        /// <summary>
        /// Вызвать печать X-отчёта
        /// </summary>
        /// <param name="xReportTask">Объект <see cref="XReportTask"/> с заданием на печать X-отчета</param>
        /// <param name="cashier">Кассир</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        /// <returns>Возвращает объект <seealso cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoXReport(XReportTask xReportTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' X-report printed.", DeviceName, deviceId);
            return GetCashRegisterData();
        }

        /// <summary>
        /// Печать текстового (нефискального) чека, информационного, рекламного, пречек и т.д.
        /// Могут присутствовать следующие теги:<para />
        /// <![CDATA[<f0/>]]> - дальше будет шрифт 0<para />
        /// <![CDATA[<f1/>]]> - дальше будет шрифт 1<para />
        /// <![CDATA[<f2/>]]> - дальше будет шрифт 2<para />
        /// <![CDATA[<papercut/>]]> - отрез ленты<para />
        /// <![CDATA[<bell/>]]> - звонок<para />
        /// <![CDATA[<barcode data="..."/>]]> - штрихкод<para />
        /// <![CDATA[<logo data="..."/>]]> - логотип<para />
        /// <![CDATA[<qrcode size="..." correction="...">...text...</qrcode>]]> - QR-код<para />
        /// </summary>
        /// <param name="printTextTask">Объект <see cref="PrintTextTask"/>, содержащий документ для печати</param>
        /// <param name="printTextTask.Document">Документ для печати (\\n)</param>
        /// <param name="user">Пользователь, выполняющий печать</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        public CashRegisterResult PrintText(PrintTextTask printTextTask, IUser user, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' printed text {2}", DeviceName, deviceId, printTextTask.Document);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Операция начала смены
        /// </summary>
        /// <param name="openSessionTask">Таск для открытия сессии на кассовом аппарате</param>
        /// <param name="cashier">Кассира</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        public CashRegisterResult DoOpenSession(OpenSessionTask openSessionTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' Session opened.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Операция внесения наличных в кассу
        /// </summary>
        /// <param name="payInTask">Таск для внесения наличных</param>
        /// <param name="cashier">Кассир</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoPayIn(PayInTask payInTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' PayIn.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Операция изъятия наличных из кассы
        /// </summary>
        /// <param name="payOutTask">Таск для изъятия наличных</param>
        /// <param name="cashier">Кассир</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoPayOut(PayOutTask payOutTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' PayOut.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Открыть денежный ящик, подключенный к фискальному регистратору
        /// </summary>
        /// <param name="cashier">Кассир</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        public void OpenDrawer(IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' Open drawer.", DeviceName, deviceId);
        }

        /// <summary>
        /// Получить состояние денежного ящика, подключенного к фискальному регистратору
        /// </summary>
        /// <returns>Открыт ли денежный ящик</returns>
        public bool IsDrawerOpened()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' IsDrawerOpened.", DeviceName, deviceId);
            return false;
        }

        /// <summary>
        /// Используется для получения поддерживаемых дополнительных операций, специфичных для определённых устройств и регионов
        /// </summary>
        /// <returns>
        /// Возвращает <see cref="QueryInfoResult"/> содержащий список поддерживаемых устройством дополнительных команд <see cref="SupportedCommand"/> ,<para />
        /// <see cref="SupportedCommand.Code"/> Уникальный идентификатор команды<para />
        /// <see cref="SupportedCommand.DisplayName"/> Название команды, которое будет отображено в меню дополнительных операций<para />
        /// и поддержку печати электронного журнала <see cref="QueryInfoResult.capQueryElectronicJournalByLastSession"/>
        /// </returns>
        public QueryInfoResult GetQueryInfo()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' Get query info.", DeviceName, deviceId);

            var supportedCommands = new List<SupportedCommand>
            {
                new ("DirectIoWithViewManager", "DirectIo with View Manager"),
                new ("DirectIoWithDocument", "DirectIo with Document"),
                new ("DirectIoWithOutputData", "DirectIo with Data"),
                new ("DirectIoWithInputOutputData", "DirectIo with Input and Output Data"),
                new ("DirectIoWithError", "DirectIo with Error")
            };
            return new QueryInfoResult(supportedCommands, false);
        }

        /// <summary>
        /// Вызвать дополнительную операцию
        /// </summary>
        /// <param name="directIoTask">Объект, содержащий параметры операции</param>
        /// <param name="user">Пользователь, выполняющий операцию</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        public DirectIoResult DirectIo(DirectIoTask directIoTask, IUser user, IViewManager viewManager)
        {
            Document document = null;
            var resultData = new Dictionary<string, string>();
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' DirectIo.", DeviceName, deviceId);

            Document CreateDocument(Dictionary<string, string> input, Dictionary<string, string> output)
            {
                var markup = new XElement(Tags.Doc,
                    new XElement(Tags.SmallFont, "This is example report text line 1"),
                    new XElement(Tags.SmallFont, "This is example report text line 2"),
                    new XElement(Tags.LargeFont, $"Input params ({input?.Count ?? 0}):"));
                if (input != null)
                {
                    foreach (var inputParam in input)
                        markup.Add(new XElement(Tags.SmallFont, $"key={inputParam.Key} value={inputParam.Value}"));
                }

                markup.Add(new XElement(Tags.LargeFont, $"Output Params ({output?.Count ?? 0}):"));
                if (output != null)
                {
                    foreach (var outputParam in output)
                        markup.Add(new XElement(Tags.SmallFont, $"key={outputParam.Key} value={outputParam.Value}"));
                }
                return markup;
            }

            switch (directIoTask.CommandCode)
            {
                case "DirectIoWithViewManager":
                    viewManager.ShowOkPopup("Message from plugin", "This is a message from the plugin that uses ViewManager");
                    break;
                case "DirectIoWithDocument":
                    break;
                case "DirectIoWithOutputData":
                    resultData.Add("key1", "value1");
                    resultData.Add("key2", "value2");
                    break;
                case "DirectIoWithInputOutputData":
                    if (directIoTask.Parameters != null)
                    {
                        foreach (var inputParam in directIoTask.Parameters)
                            resultData.Add($"{inputParam.Key}_response", $"{inputParam.Value}_response");
                    }
                    break;
                case "DirectIoWithError":
                    throw new DeviceException("This is a DeviceException from the plugin");
                default:
                    throw new DeviceException("Command not supported");
            }
            document = CreateDocument(directIoTask.Parameters, resultData);
            return new DirectIoResult(document, resultData);
        }

        /// <summary>
        /// Используется для формирования счёта
        /// </summary>
        /// <param name="billTask"> Аналогичен параметру <see cref="ChequeTask"/> в операции печати чека <see cref="DoCheque"/>,
        /// за исключением отсутствия оплат</param>
        /// <param name="viewManager">Объект <see cref="IViewManager"/> для показа диалоговых окон</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoBillCheque(BillTask billTask, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' BillCheque.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Переводит дисплей покупателя в режим ожидания, если он присутствует
        /// </summary>
        /// <param name="timeToIdle">Время до отключения дисплея</param>
        public void CustomerDisplayIdle(TimeSpan timeToIdle)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' CustomerDisplayIdle.", DeviceName, deviceId);
        }

        /// <summary>
        /// Отобразить текст на дисплее покупателя, если он присутствует
        /// </summary>
        /// <param name="task"> параметр содержит 4 поля с текстом <see cref="CustomerDisplayTextTask.BottomLeft"/>, 
        /// <see cref="CustomerDisplayTextTask.BottomRight"/>, 
        /// <see cref="CustomerDisplayTextTask.TopLeft"/>, <see cref="CustomerDisplayTextTask.TopRight"/></param>
        public void CustomerDisplayText(CustomerDisplayTextTask task)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' CustomerDisplayText.", DeviceName, deviceId);
        }

        /// <summary>
        /// Возвращает параметры и возможности печатающего устройства <see cref="CashRegisterDriverParameters"/>
        /// </summary>
        /// <returns>Содержит следующие данные: <para/>
        /// <see cref="CashRegisterDriverParameters.IsCancellationSupported"/> Поддерживает ли ФР операцию Аннулирования<para />
        /// <see cref="CashRegisterDriverParameters.CanUseFontSizes"/>Поддерживает ли ККМ использование шрифтов для печати текста<para />
        /// <see cref="CashRegisterDriverParameters.CanPrintQRCode"/>Поддерживает ли ККМ печать QR-кодов<para />
        /// <see cref="CashRegisterDriverParameters.CanPrintLogo"/>Поддерживает ли ККМ печать логотипов<para />
        /// <see cref="CashRegisterDriverParameters.CanPrintBarcode"/>Поддерживает ли ККМ печать простых штрихкодов<para />
        /// <see cref="CashRegisterDriverParameters.ZeroCashOnClose"/>Обнуляет ли ККМ сумму наличных в кассе при закрытии смены<para />
        /// <see cref="CashRegisterDriverParameters.CanPrintText"/>Поддерживает ли ККМ печать текста<para />
        /// Ширину строки для каждого шрифта: <see cref="CashRegisterDriverParameters.Font0Width"/>, 
        /// <see cref="CashRegisterDriverParameters.Font1Width"/>, <see cref="CashRegisterDriverParameters.Font2Width"/> <para />
        /// <see cref="CashRegisterDriverParameters.IsBillTaskSupported"/>Поддерживается ли ФР печать пречека через команду "Счет"<para />
        /// </returns>
        public CashRegisterDriverParameters GetCashRegisterDriverParameters()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' GetCashRegisterDriverParameters.", DeviceName, deviceId);
            return new CashRegisterDriverParameters
            {
                CanPrintText = true,
                CanPrintBarcode = true,
                CanPrintLogo = true,
                CanPrintQRCode = true,
                CanUseFontSizes = true,
                Font0Width = 44,
                Font1Width = 42,
                Font2Width = 22,
                IsCancellationSupported = true,
                ZeroCashOnClose = true,
                IsBillTaskSupported = false,
                IsBuyChequeSupported = true,
                IsMultipleMarkingCodesPerUnitSupported = true
            };
        }

        #region IFfdCashRegister implementation

        public CheckFfd12MarkingResult CheckFfd12Marking(CheckFfd12MarkingTask task, IUser cashier)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' CheckFfd12Marking.", DeviceName, deviceId);
            return new CheckFfd12MarkingResult(false, "", 1, 2, 3, 4, 5, 6);
        }

        public void DoFfd10Correction(Ffd10CorrectionTask task, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' DoFfd10Correction.", DeviceName, deviceId);
        }

        public CashRegisterResult DoFfd11Correction(Ffd11CorrectionTask task, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' DoFfd11Correction.", DeviceName, deviceId);
            return GetCashRegisterData();
        }

        public FiscalTagsResult GetDocumentFiscalTags(GetFiscalTagsTask task, IUser cashier)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' GetDocumentFiscalTags.", DeviceName, deviceId);
            return new FiscalTagsResult(1, [new FiscalTag {Code = 1, ChildTags = [new FiscalTag { Code = 11, Value = "Child1" },
                new FiscalTag {Code = 12, Value ="Child2"}] }, new FiscalTag {Code = 2, Value ="Parent2"}]);
        }

        #endregion

        #region IAutosearchCashRegister implementation
        public AutosearchDeviceSettingsResult SearchSettings(AutosearchDeviceSettingsTask task)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' SearchSettings.", DeviceName, deviceId);
            return new AutosearchDeviceSettingsResult([new DeviceStringSetting {Name = "Setting1", Value = "Test"},
                new DeviceBooleanSetting {Name = "Setting2", Value = true}]);
        }

        public ConfigureDeviceSettingsResult ConfigureSettings(ConfigureDeviceSettingsTask task)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' ConfigureSettings.", DeviceName, deviceId);
            return new ConfigureDeviceSettingsResult(true);
        }
        #endregion
    }
}