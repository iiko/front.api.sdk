using System;
using System.Collections.Generic;
using System.Linq;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.SamplePlugin.WpfHelpers;

namespace Resto.Front.Api.SamplePlugin.CustomData;

internal class CustomDataWindowViewModel : ViewModelBase, IDisposable
{
    private IDisposable customDataEventsSubscription;
    private IDisposable beforeCustomDataClearSubscription;

    public event Action<string> ShowMessage; 

    private string key;
    public string Key
    {
        get => key;
        set
        {
            key = value;
            RaisePropertyChanged();
        }
    }

    private string valueToAddOrUpdate;
    public string ValueToAddOrUpdate
    {
        get => valueToAddOrUpdate;
        set
        {
            valueToAddOrUpdate = value;
            RaisePropertyChanged();
        }
    }

    private string valueToGetOrRemoved;
    public string ValueToGetOrRemoved
    {
        get => valueToGetOrRemoved;
        set
        {
            valueToGetOrRemoved = value;
            RaisePropertyChanged();
        }
    }

    private string getAllCustomDataResult;
    public string GetAllCustomDataResult
    {
        get => getAllCustomDataResult;
        set
        {
            getAllCustomDataResult = value;
            RaisePropertyChanged();
        }
    }

    private bool isSubscribedToEvents;
    public bool IsSubscribedToEvents
    {
        get => isSubscribedToEvents;
        set
        {
            isSubscribedToEvents = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(SubscribedToEventsLabel));
        }
    }

    public string SubscribedToEventsLabel => isSubscribedToEvents ? "Unsubscribe events" : "Subscribe events";

    private string customDataEventsFlowText;
    public string CustomDataEventsFlowText
    {
        get => customDataEventsFlowText;
        set
        {
            customDataEventsFlowText = value;
            RaisePropertyChanged();
        }
    }

    private bool isBeforeClearSubscribed;
    public bool IsBeforeClearSubscribed
    {
        get => isBeforeClearSubscribed;
        set
        {
            isBeforeClearSubscribed = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(BeforeClearSubscriptionLabel));
        }
    }

    private string beforeClearFlowText;
    public string BeforeClearFlowText
    {
        get => beforeClearFlowText;
        set
        {
            beforeClearFlowText = value;
            RaisePropertyChanged();
        }
    }

    public string BeforeClearSubscriptionLabel => IsBeforeClearSubscribed ? "Unsubscribe Before Clear event" : "Subscribe Before Clear event";

    private RelayCommand addOrUpdateCustomDataCommand;
    public RelayCommand AddOrUpdateCustomDataCommand => addOrUpdateCustomDataCommand ??= new RelayCommand(_ => AddOrUpdateCustomData(), AddOrUpdateCanExecute);

    private RelayCommand tryGetCustomDataCommand;
    public RelayCommand TryGetCustomDataCommand => tryGetCustomDataCommand ??= new RelayCommand(_ => TryGetCustomData(), TryGetCustomDataCanExecute);

    private RelayCommand tryRemoveCustomDataCommand;
    public RelayCommand TryRemoveCustomDataCommand => tryRemoveCustomDataCommand ??= new RelayCommand(_ => TryRemoveCustomData(), TryRemoveCustomDataCanExecute);

    private RelayCommand getAllCustomDataCommand;
    public RelayCommand GetAllCustomDataCommand => getAllCustomDataCommand ??= new RelayCommand(_ => GetAllCustomData());

    private RelayCommand clearCustomDataCommand;
    public RelayCommand ClearCustomDataCommand => clearCustomDataCommand ??= new RelayCommand(_ => ClearCustomData());

    private RelayCommand customDataEventsCommand;
    public RelayCommand CustomDataEventsCommand => customDataEventsCommand ??= new RelayCommand(_ => PerformCustomDataEventsSubscribe());

    private RelayCommand beforeCustomDataClearCommand;
    public RelayCommand BeforeCustomDataClearCommand => beforeCustomDataClearCommand ??= new RelayCommand(_ => PerformSubscribeBeforeCustomDataClear());

    private bool AddOrUpdateCanExecute(object arg)
    {
        return !string.IsNullOrWhiteSpace(Key) && !string.IsNullOrWhiteSpace(ValueToAddOrUpdate);
    }

    private bool TryGetCustomDataCanExecute(object arg)
    {
        return !string.IsNullOrWhiteSpace(key);
    }

    private bool TryRemoveCustomDataCanExecute(object arg)
    {
        return !string.IsNullOrWhiteSpace(Key);
    }

    private void AddOrUpdateCustomData()
    {
        try
        {
            PluginContext.Operations.AddOrUpdateCustomData(Key, ValueToAddOrUpdate);
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private void TryGetCustomData()
    {
        try
        {
            ValueToGetOrRemoved = PluginContext.Operations.TryGetCustomData(Key);
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private void TryRemoveCustomData()
    {
        try
        {
            ValueToGetOrRemoved = PluginContext.Operations.TryRemoveCustomData(Key).ToString();
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private void GetAllCustomData()
    {
        try
        {
            var allData = PluginContext.Operations.GetAllCustomData();
            GetAllCustomDataResult = string.Join(Environment.NewLine, allData.Select(kvp => $"{kvp.Key} - {kvp.Value}"));
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private void ClearCustomData()
    {
        try
        {
            PluginContext.Operations.ClearCustomData();
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private void PerformCustomDataEventsSubscribe()
    {
        try
        {
            if (IsSubscribedToEvents)
            {
                customDataEventsSubscription?.Dispose();
                CustomDataEventsFlowText = string.Empty;
            }
            else
            {
                customDataEventsSubscription = PluginContext.Notifications.CustomDataChanged.Subscribe(OnCustomDataChanged);
            }
            IsSubscribedToEvents = !IsSubscribedToEvents;
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private void OnCustomDataChanged(CustomDataChangedEventArgs e)
    {
        CustomDataEventsFlowText += $"({e.EventType}) {e.Key} - {e.Value}";
    }

    private void PerformSubscribeBeforeCustomDataClear()
    {
        try
        {
            if (IsBeforeClearSubscribed)
            {
                beforeCustomDataClearSubscription?.Dispose();
                BeforeClearFlowText = string.Empty;
            }
            else
            {
                beforeCustomDataClearSubscription = PluginContext.Notifications.BeforeCustomDataClear.Subscribe(OnBeforeCustomDataClear);
            }
            IsBeforeClearSubscribed = !IsBeforeClearSubscribed;
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private IReadOnlyCollection<string> OnBeforeCustomDataClear(IDictionary<string, string> staleData)
    {
        BeforeClearFlowText +=
            $"{DateTime.Now} -- OnBeforeCustomDataClear raised! With {staleData.Keys.Count} stale data\r\n";

        // Nothing to save
        return null;
    }

    public void Dispose()
    {
        customDataEventsSubscription?.Dispose();
        beforeCustomDataClearSubscription?.Dispose();
    }
}