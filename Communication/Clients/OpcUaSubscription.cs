using Opc.Ua;
using Opc.Ua.Client;

namespace InjectionMoldingMachineDataAcquisitionService.Communication.Clients;
public class OpcUaSubscription
{
    private readonly Subscription _subscription;
    private readonly List<OpcUaNotificationHandler> notificationHandlers = new();

    public OpcUaSubscription(Subscription subscription)
    {
        _subscription = subscription;
    }

    public void AddMonitorItem(string nodeId, string itemName, int samplingInterval, List<Action<MetricMessage>> handler)
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

        notificationHandlers.Add(notificationHandler);
    }


    private class OpcUaNotificationHandler
    {
        private event Action<MetricMessage>? _messageHandlers;

        public OpcUaNotificationHandler(List<Action<MetricMessage>> messageHandler, MonitoredItem monitoredItem)
        {
            messageHandler.ForEach(h => _messageHandlers += h);

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