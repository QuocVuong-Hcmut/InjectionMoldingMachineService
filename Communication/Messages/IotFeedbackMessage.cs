namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class IotFeedbackMessage
{
    public EFeedback Mess { get; set; }

    public IotFeedbackMessage(EFeedback mess)
    {
        Mess = mess;
    }
}
