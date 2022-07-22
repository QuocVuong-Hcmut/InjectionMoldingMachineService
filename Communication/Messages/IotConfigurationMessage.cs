namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class IotConfigurationMessage
{
    public DateTime Timestamp { get; set; }
    public string MoldId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public double CycleTime { get; set; }
}
