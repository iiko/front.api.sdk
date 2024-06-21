using System;
using System.Linq;
using System.Text;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.SamplePlugin.WpfHelpers;

namespace Resto.Front.Api.SamplePlugin.CustomData;

internal class CustomDataWindowViewModel : ViewModelBase, IDisposable
{
    private IDisposable subscription;

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

    private bool isSubscribed;
    public bool IsSubscribed
    {
        get => isSubscribed;
        set
        {
            isSubscribed = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(SubscribedText));
        }
    }

    public string SubscribedText => isSubscribed ? "Unsubscribe" : "Subscribe";

    private readonly StringBuilder subscriptionFlowBuilder = new();
    private string subscriptionFlowText;
    public string SubscriptionFlowText
    {
        get => subscriptionFlowText;
        set
        {
            subscriptionFlowText = value;
            RaisePropertyChanged();
        }
    }

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

    private RelayCommand subscribeCommand;
    public RelayCommand SubscribeCommand => subscribeCommand ??= new RelayCommand(_ => PerformSubscribe());

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

    private void PerformSubscribe()
    {
        try
        {
            if (IsSubscribed)
            {
                subscription?.Dispose();
                subscriptionFlowBuilder.Clear();
                SubscriptionFlowText = string.Empty;
            }
            else
            {
                subscription = PluginContext.Notifications.CustomDataChanged.Subscribe(OnCustomDataChanged);
            }
            IsSubscribed = !IsSubscribed;
        }
        catch (Exception e)
        {
            ShowMessage?.Invoke(e.ToString());
        }
    }

    private void OnCustomDataChanged(CustomDataChangedEventArgs e)
    {
        subscriptionFlowBuilder.AppendLine($"({e.EventType}) {e.Key} - {e.Value}");
        SubscriptionFlowText = subscriptionFlowBuilder.ToString();
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }
}