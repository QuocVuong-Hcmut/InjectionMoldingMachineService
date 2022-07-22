namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class UaMessage
{
    public string Name { get; private set; }
    public object Value { get; private set; }

    public UaMessage(string name, object value)
    {
        Name = name;
        Value = value;
    }
}
