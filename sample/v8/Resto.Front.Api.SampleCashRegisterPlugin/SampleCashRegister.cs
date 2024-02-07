using System;
using System.Collections.Generic;
using System.Linq;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Device;
using Resto.Front.Api.Data.Device.Results;
using Resto.Front.Api.Data.Device.Settings;
using Resto.Front.Api.Data.Device.Tasks;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Devices;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SampleCashRegisterPlugin
{
    internal sealed class SampleCashRegister : Devices.ICashRegister
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
        public CashRegisterResult DoCheque(ChequeTask chequeTask, IViewManager viewManager, IOperationDataContext operationDataContext)
        {
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

            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. Cheque printed.", DeviceName, deviceId);

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
            return new CashRegisterResult(null, null, null, "", null, null, null, "", "", "", null);
        }

        /// <summary>
        /// Вызвать печать Z-отчета (Закрытие смены)
        /// </summary>
        /// <param name="cafeSession">Закрываемая кассовая смена</param>
        /// <param name="zReportTask">Таск для печати Z-отчета</param>
        /// <param name="user">Пользователь проводящий печать</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoZReport(ICafeSession cafeSession, ZReportTask zReportTask, IUser user, IViewManager viewManager)
        {
            CheckStarted();

            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. Z-report printed.", DeviceName, deviceId);
            return GetCashRegisterData();
        }

        /// <summary>
        /// Проверить состояние устройства
        /// </summary>
        /// <param name="statusFields">Список запрашиваемых полей</param>
        /// <returns>Возвращает экземпляры <see cref="CashRegisterStatus"/>, с заполненными запрошенными полями, либо null, если значение не заполнено.
        /// </returns>
        public CashRegisterStatus GetCashRegisterStatus(GetCashRegisterStatusTask getCashRegisterStatusTask)
        {
            return new CashRegisterStatus();
        }

        /// <summary>
        /// Вызвать печать X-отчёта
        /// </summary>
        /// <param name="cashier">Кассира</param>
        /// <returns>Возвращает объект <seealso cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoXReport(XReportTask xReportTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. Z-report printed.", DeviceName, deviceId);
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
        /// <param name="printTextTask.Document">Документ для печати (\\n)</param>
        public CashRegisterResult PrintText(PrintTextTask printTextTask, IUser user)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})' printed text {2}", DeviceName, deviceId, printTextTask.Document);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Операция начала смены
        /// </summary>
        /// <param name="openSessionTask">Таск для открытия сессии на кассовом аппарате</param>
        /// <param name="cashier">Кассира</param>
        public CashRegisterResult DoOpenSession(OpenSessionTask openSessionTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. Session opened.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Операция внесения наличных в кассу
        /// </summary>
        /// <param name="payInTask">Таск для внесения наличных</param>
        /// <param name="cashier">Кассир</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoPayIn(PayInTask payInTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. PayIn.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Операция изъятия наличных из кассы
        /// </summary>
        /// <param name="payOutTask">Таск для изъятия наличных</param>
        /// <param name="cashier">Кассир</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoPayOut(PayOutTask payOutTask, IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. PayOut.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Открыть денежный ящик, подключенный к фискальному регистратору
        /// </summary>
        /// <param name="cashier">Кассир</param>
        public void OpenDrawer(IUser cashier, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. Open drawer.", DeviceName, deviceId);
        }

        /// <summary>
        /// Получить состояние денежного ящика, подключенного к фискальному регистратору
        /// </summary>
        /// <returns>Открыт ли денежный ящик</returns>
        public bool IsDrawerOpened()
        {
            return false;
        }

        /// <summary>
        /// Используется для получения поддерживаемых дополнительных операций, специфичных для определённых устройств и регионов
        /// </summary>
        /// <returns>
        /// Возвращает <see cref="QueryInfoResult"/> содержащий список поддерживаемых устройством дополнительных команд <see cref="SupportedCommand"/> ,<para />
        /// <see cref="SupportedCommand.Name"/> Имя команды, например: "TestPrinter"<para />
        /// <see cref="SupportedCommand.ResourceName"/> Название ресурса команды, например: "ExtFiscalCommandExtendedTest"<para />
        /// <see cref="SupportedCommand.Parameters"/> Список параметров команды типа <see cref="RequiredParameter"/>, содержит поля:<para />
        /// <see cref="RequiredParameter.Name"/> Имя параметра, например: "print"<para />
        /// <see cref="RequiredParameter.ResourceName"/> Название ресурса параметра, например: "ExtFiscalParameterPrint"<para />
        /// <see cref="RequiredParameter.ResourceTip"/> Название подсказки параметра, например: "ExtFiscalParameterPrintTip"<para />
        /// <see cref="RequiredParameter.Type"/> Имя параметра, например: "bool"<para />
        /// и поддержку печати электронного журнала <see cref="QueryInfoResult.capQueryElectronicJournalByLastSession"/>
        /// </returns>
        public QueryInfoResult GetQueryInfo()
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. Get query info.", DeviceName, deviceId);

            return new QueryInfoResult(new List<SupportedCommand>(), false);
        }

        /// <summary>
        /// Вызвать дополнительную операцию
        /// </summary>
        /// <returns>
        /// Результат выполнения типа <see cref="Document"/>
        /// со списком строк <see cref="Document.Lines"/>, содержащими текст результата<para />
        /// При ошибке выкидывать исключение
        /// </returns>
        public DirectIoResult DirectIo(DirectIoTask directIoTask, IUser user)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. DirectIo.", DeviceName, deviceId);

            return new DirectIoResult(new Document(), "123123123");
        }

        public CheckFfd12MarkingResult CheckFfd12Marking([NotNull] CheckFfd12MarkingTask task)
        {
            PluginContext.Log.WarnFormat("Device: '{0} ({1})'. CheckFfd12Marking not supported.", DeviceName, deviceId);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Используется для формирования счёта
        /// </summary>
        /// <param name="billTask"> Аналогичен параметру <see cref="ChequeTask"/> в операции печати чека <see cref="DoCheque"/>,
        /// за исключением отсутствия оплат</param>
        /// <returns>Возвращает объект <see cref="CashRegisterResult"/>
        /// При ошибке выкидывать исключение
        /// </returns>
        public CashRegisterResult DoBillCheque(BillTask billTask, IViewManager viewManager)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. BillCheque.", DeviceName, deviceId);

            return GetCashRegisterData();
        }

        /// <summary>
        /// Переводит дисплей покупателя в режим ожидания, если он присутствует
        /// </summary>
        /// <param name="timeToIdle">Время до отключения дисплея</param>
        public void CustomerDisplayIdle(TimeSpan timeToIdle)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. CustomerDisplayIdle.", DeviceName, deviceId);
        }

        /// <summary>
        /// Отобразить текст на дисплее покупателя, если он присутствует
        /// </summary>
        /// <param name="task"> параметр содержит 4 поля с текстом <see cref="CustomerDisplayTextTask.BottomLeft"/>, 
        /// <see cref="CustomerDisplayTextTask.BottomRight"/>, 
        /// <see cref="CustomerDisplayTextTask.TopLeft"/>, <see cref="CustomerDisplayTextTask.TopRight"/></param>
        public void CustomerDisplayText(CustomerDisplayTextTask task)
        {
            PluginContext.Log.InfoFormat("Device: '{0} ({1})'. CustomerDisplayText.", DeviceName, deviceId);
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
            };
        }
    }
}