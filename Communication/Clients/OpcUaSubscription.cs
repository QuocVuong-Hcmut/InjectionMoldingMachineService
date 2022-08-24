using Opc.Ua;
using Opc.Ua.Client;

namespace InjectionMoldingMachineDataAcquisitionService.Communication.Clients;
public class OpcUaSubscription: IDisposable
{
    private readonly Subscription _subscription;
    private readonly List<OpcUaNotificationHandler> _notificationHandlers = new();
    
    public OpcUaSubscription(Subscription subscription)
    {
        _subscription = subscription;
    }

    public void AddMonitorItem(string nodeId, string itemName,
        int samplingInterval, List<Action<MetricMessage>> handler)
    {
        MonitoredItem item = new(_subscription.DefaultItem)
        {
            DisplayName = itemName,
            StartNodeId = new NodeId(nodeId),
            AttributeId = Attributes.Value,
            SamplingInterval = samplingInterval
        };
        var notificationHandler = new OpcUaNotificationHandler(handler, item);

        _subscription.AddItem(item);
        _subscription.ApplyChanges();
        _notificationHandlers.Add(notificationHandler);
    }

    public void SuspendMonitoredItemSubscription(string itemName)
    {
        var notificationHandler = _notificationHandlers.FirstOrDefault(
            h => h.MonitoredItem.DisplayName == itemName);

        if (notificationHandler is not null)
        {
            _subscription.RemoveItem(notificationHandler.MonitoredItem);
        }
    }

    public void ContinueMonitoredItemSubscription(string itemName)
    {
        var notificationHandler = _notificationHandlers.FirstOrDefault(
            h => h.MonitoredItem.DisplayName == itemName);

        if (notificationHandler is not null)
        {
            if (_subscription.MonitoredItems.Any(m => m.DisplayName == itemName))
            {
                return;
            }
            
            _subscription.AddItem(notificationHandler.MonitoredItem);
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }

    private class OpcUaNotificationHandler
    {
        private event Action<MetricMessage>? _messageHandlers;
        public MonitoredItem MonitoredItem { get; private set; }

        public OpcUaNotificationHandler(List<Action<MetricMessage>> messageHandler, MonitoredItem monitoredItem)
        {
            messageHandler.ForEach(h => _messageHandlers += h);
            MonitoredItem = monitoredItem;
            monitoredItem.Notification += HandleNotification;
        }
        private void HandleNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification? notification = e.NotificationValue as MonitoredItemNotification;

            if (notification is not null)
            {
                _messageHandlers?.Invoke(new MetricMessage(monitoredItem.DisplayName, notification.Value.Value, notification.Value.SourceTimestamp));
            }
        }
    }
}