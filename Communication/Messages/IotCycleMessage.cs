namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class IotCycleMessage
{
    public DateTime Timestamp { get; set; }
    public double CycleTime { get; set; }
    public double OpenTime { get; set; }
    public int Mode { get; set; }
    public int CounterShot { get; set; }
    public string MoldId { get; set; }
    public string ProductId { get; set; }
    public double SetCycle { get; set; }

    public IotCycleMessage(DateTime timestamp, double cycleTime, double openTime, int mode, int counterShot, string moldId, string productId, double setCycle)
    {
        Timestamp = timestamp;
        CycleTime = cycleTime;
        OpenTime = openTime;
        Mode = mode;
        CounterShot = counterShot;
        MoldId = moldId;
        ProductId = productId;
        SetCycle = setCycle;
    }
}
