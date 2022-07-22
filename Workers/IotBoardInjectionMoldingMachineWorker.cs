using MQTTnet.Client;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;
using MqttClient = InjectionMoldingMachineDataAcquisitionService.Communication.Clients.MqttClient;

namespace InjectionMoldingMachineDataAcquisitionService.Workers;
public class IotBoardInjectionMoldingMachineWorker : BackgroundService
{
    private readonly MqttClient _mqttClient;
    private readonly IBusControl _busControl;
    private readonly Timer _reconnectTimer;

    public IotBoardInjectionMoldingMachineWorker(MqttClient mqttClient, IBusControl busControl)
    {
        _mqttClient = mqttClient;
        _busControl = busControl;
        _reconnectTimer = new Timer(10000);
        _reconnectTimer.Elapsed += ReconnectTimerElapsed;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _mqttClient.ApplicationMessageReceived += OnMqttClientMessageReceivedAsync;
        _mqttClient.Disconnected += OnDisconnected;

        await ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        _reconnectTimer.Enabled = false;
        try
        {
            await _mqttClient.ConnectAsync();

            await _mqttClient.Subscribe("IMM/+/CycleMessage");
            await _mqttClient.Subscribe("IMM/+/MachineStatus");
            await _mqttClient.Subscribe("IMM/+/Feedback");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MQTT connection failed: {ex.Message}");
            _reconnectTimer.Enabled = true;
        }
    }

    private async Task OnMqttClientMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payloadMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

        Console.WriteLine($"{topic}: {payloadMessage}");

        var topicPaths = topic.Split('/');
        var machineId = topicPaths[1];
        var messageType = topicPaths[2];

        await HandleMessageAsync(machineId, messageType, payloadMessage);
    }

    private async Task HandleMessageAsync(string machineId, string messageType, string payloadMessage)
    {
        switch (messageType)
        {
            case "CycleMessage":
                {
                    await HandleCycleMessageAsync(machineId, payloadMessage);
                }
                break;
            case "MachineStatus":
                {
                    await HandleMachineStatusMessageAsync(machineId, payloadMessage);
                }
                break;
            case "Feedback":
                {
                    await HandleFeedbackMessageAsync(machineId, payloadMessage);
                }
                break;
        }
    }

    private async Task HandleCycleMessageAsync(string machineId, string payloadMessage)
    {
        var iotCycleMessage = JsonConvert.DeserializeObject<IotCycleMessage>(payloadMessage);
        if (iotCycleMessage is null)
        {
            return;
        }

        var cycleMessage = new CycleMessage(machineId, iotCycleMessage.Timestamp, iotCycleMessage.CycleTime, iotCycleMessage.OpenTime, iotCycleMessage.Mode, iotCycleMessage.CounterShot, iotCycleMessage.MoldId, iotCycleMessage.ProductId, iotCycleMessage.SetCycle);
        await _busControl.Publish(cycleMessage);

        Console.WriteLine($"{cycleMessage.Timestamp}, {cycleMessage.CycleTime}");
    }

    private async Task HandleMachineStatusMessageAsync(string machineId, string payloadMessage)
    {
        var iotMachineStatus = JsonConvert.DeserializeObject<IotMachineStatusMessage>(payloadMessage);
        if (iotMachineStatus is null)
        {
            return;
        }

        var machineStatus = new MachineStatusMessage(machineId, iotMachineStatus.Timestamp, iotMachineStatus.MachineStatus);
        Console.WriteLine(machineStatus.Timestamp.ToString(), machineStatus.MachineStatus);
        await _busControl.Publish(machineStatus);
    }

    private async Task HandleFeedbackMessageAsync(string machineId, string payloadMessage)
    {
        var iotFeedback = JsonConvert.DeserializeObject<IotFeedbackMessage>(payloadMessage);
        if (iotFeedback is null)
        {
            return;
        }

        var feedback = new FeedbackMessage(machineId, iotFeedback.Mess);
        await _busControl.Publish(feedback);

        if (feedback.Mess == EFeedback.SychTime)
        {
            var now = DateTime.Now;
            var synchonizeTimeMessage = new SynchronizeTimeMessage(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            var payload = JsonConvert.SerializeObject(synchonizeTimeMessage);

            string topic = "IMM/" + machineId + "/SyncTime";
            await _mqttClient.Publish(topic, payload, false);
        }
    }

    private async void ReconnectTimerElapsed(object? sender, ElapsedEventArgs args)
    {
        if (!_mqttClient.IsConnected)
        {
            await ConnectAsync();
        }
        else
        {
            _reconnectTimer.Enabled = false;
        }
    }

    private Task OnDisconnected(MqttClientDisconnectedEventArgs args)
    {
        _reconnectTimer.Enabled = true;
        return Task.CompletedTask;
    }
}
