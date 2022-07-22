namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class FeedbackMessage
{
    public string MachineId { get; set; }
    public EFeedback Mess { get; set; }

    public FeedbackMessage(string machineId, EFeedback mess)
    {
        MachineId = machineId;
        Mess = mess;
    }
}
