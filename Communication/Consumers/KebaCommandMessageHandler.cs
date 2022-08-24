namespace InjectionMoldingMachineDataAcquisitionService.Communication.Consumers;
public class KebaConfigurationMessageConsumer : IConsumer<ConfigurationMessage>
{
    private readonly KebaConfigurationMessageObserver _observer;

    public KebaConfigurationMessageConsumer(KebaConfigurationMessageObserver observer)
    {
        _observer = observer;
    }

    public Task Consume(ConsumeContext<ConfigurationMessage> context)
    {
        var message = context.Message;
        _observer.ReceiveConfigurationMessage(message);
        return Task.CompletedTask;
    }
}
