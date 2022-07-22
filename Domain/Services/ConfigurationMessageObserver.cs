namespace InjectionMoldingMachineDataAcquisitionService.Domain.Services;
public class KebaConfigurationMessageObserver
{
    public event Action<ConfigurationMessage>? ConfigurationMessageReceived;

    public void ReceiveConfigurationMessage(ConfigurationMessage configurationMessage)
    {
        ConfigurationMessageReceived?.Invoke(configurationMessage);
    }
}
