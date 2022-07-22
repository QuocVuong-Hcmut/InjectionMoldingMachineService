namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class MachineStatusMessage
{
    public string MachineId { get; set; }
    public DateTime Timestamp { get; set; }
    public EMachineStatus MachineStatus { get; set; }

    public MachineStatusMessage(string machineId, DateTime timestamp, EMachineStatus machineStatus)
    {
        MachineId = machineId;
        Timestamp = timestamp;
        MachineStatus = machineStatus;
    }
}
