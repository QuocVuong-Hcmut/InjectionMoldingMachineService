namespace InjectionMoldingMachineDataAcquisitionService.Communication.Consumers;
public class ConfigurationMessageConsumer : IConsumer<ConfigurationMessage>
{
    private readonly MqttClient _mqttClient;

    public ConfigurationMessageConsumer(MqttClient mqttClient)
    {
        _mqttClient = mqttClient;
    }

    public async Task Consume(ConsumeContext<ConfigurationMessage> context)
    {
        Console.WriteLine("ConfigurationMessageReceived");
        var message = context.Message;

        string topic = "IMM/" + message.MachineId + "/ConfigMess";

        var payload = JsonConvert.SerializeObject(new IotConfigurationMessage
        {
            Timestamp = message.Timestamp,
            MoldId = message.MoldId,
            ProductId = message.ProductId,
            CycleTime = message.CycleTime
        });

        await _mqttClient.Publish(topic, payload, true);
    }
}
