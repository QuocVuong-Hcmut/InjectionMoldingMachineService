namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class IotCommandMessage
{
    public DateTime Timestamp { get; set; }
    public ECommand Command { get; set; }
}
