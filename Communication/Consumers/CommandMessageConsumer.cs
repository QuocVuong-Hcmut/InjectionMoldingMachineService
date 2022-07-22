namespace InjectionMoldingMachineDataAcquisitionService.Communication.Consumers;
public class CommandMessageConsumer: IConsumer<CommandMessage>
{
    private readonly MqttClient _mqttClient;

    public CommandMessageConsumer(MqttClient mqttClient)
    {
        _mqttClient = mqttClient;
    }

    public async Task Consume(ConsumeContext<CommandMessage> context)
    {
        Console.WriteLine("CommandMessageReceived");
        var message = context.Message;

        string topic = "IMM/" + message.MachineId + "/DAMess";

        var payload = JsonConvert.SerializeObject(new IotCommandMessage
        {
            Timestamp = message.Timestamp,
            Command = message.Command
        });

        await _mqttClient.Publish(topic, payload, false);
    }
}
