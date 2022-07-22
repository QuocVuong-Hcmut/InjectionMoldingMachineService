using MQTTnet;
using MQTTnet.Client;
using System.Timers;

namespace InjectionMoldingMachineDataAcquisitionService.Communication.Clients;
public class MqttClient
{
    public MqttOptions Options { get; set; }
    public bool IsConnected => _mqttClient is not null && _mqttClient.IsConnected;

    public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceived;
    public event Func<MqttClientDisconnectedEventArgs, Task>? Disconnected;

    private IMqttClient? _mqttClient;

    public MqttClient(IOptions<MqttOptions> options)
    {
        Options = options.Value;
    }

    public async Task ConnectAsync()
    {
        if (_mqttClient is not null)
        {
            await _mqttClient.DisconnectAsync();
            _mqttClient.Dispose();
        }

        _mqttClient = new MqttFactory().CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(Options.Host, Options.Port)
            .WithTimeout(TimeSpan.FromSeconds(Options.CommunicationTimeout))
            .WithClientId(Options.ClientId)
            .WithCredentials(Options.UserName, Options.Password)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(Options.KeepAliveInterval));

        _mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceived;
        _mqttClient.DisconnectedAsync += Disconnected;

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(Options.CommunicationTimeout));
        var result = await _mqttClient.ConnectAsync(mqttClientOptions.Build(), timeout.Token);

        if (result.ResultCode != MqttClientConnectResultCode.Success)
        {
            throw new MqttConnectionException($"{result.ResultCode}: {result.ReasonString}");
        }
    }

    public async Task DisconnectAsync()
    {
        await _mqttClient.DisconnectAsync();
    }

    public async Task Subscribe(string topic)
    {
        if (_mqttClient is null)
        {
            throw new InvalidOperationException("MQTT Client is not connected.");
        }

        var topicFilter = new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .Build();

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topicFilter)
            .Build();

        var result = await _mqttClient.SubscribeAsync(subscribeOptions);

        foreach (var subscription in result.Items)
        {
            if (subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0 &&
                subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS1 &&
                subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS2)
            {
                Console.WriteLine($"MQTT Client Subscription {subscription.TopicFilter.Topic} Failed: {subscription.ResultCode}");
            }
        }
    }

    public async Task Publish(string topic, string payload, bool retainFlag)
    {
        if (_mqttClient is null)
        {
            throw new InvalidOperationException("MQTT Client is not connected.");
        }

        var applicationMessageBuilder = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithRetainFlag(retainFlag)
            .WithPayload(payload);

        var applicationMessage = applicationMessageBuilder.Build();

        var result = await _mqttClient.PublishAsync(applicationMessage);

        if (result.ReasonCode != MqttClientPublishReasonCode.Success)
        {
            Console.WriteLine($"MQTT Client Publish {applicationMessage.Topic} Failed: {result.ReasonCode}");
        }
    }
}
