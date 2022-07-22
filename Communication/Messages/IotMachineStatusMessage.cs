namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class IotMachineStatusMessage
{
    public DateTime Timestamp { get; set; }
    public EMachineStatus MachineStatus { get; set; }

    public IotMachineStatusMessage(DateTime timestamp, EMachineStatus machineStatus)
    {
        Timestamp = timestamp;
        MachineStatus = machineStatus;
    }
}
