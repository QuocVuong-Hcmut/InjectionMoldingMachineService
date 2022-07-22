namespace InjectionMoldingMachineDataAcquisitionService.Communication.Exceptions;
public class MqttConnectionException : Exception
{
    public MqttConnectionException(string? message) : base(message)
    {
    }
}
