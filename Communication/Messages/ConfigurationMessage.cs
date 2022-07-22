namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class ConfigurationMessage
{
    public string MachineId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string MoldId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public double CycleTime { get; set; }
}
