namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class MetricMessage
{
    public string Name { get; private set; }
    public object Value { get; private set; }
    public DateTime Timestamp { get; private set; }

    public MetricMessage(string name, object value, DateTime timestamp)
    {
        Name = name;
        Value = value;
        Timestamp = timestamp;
    }
}
