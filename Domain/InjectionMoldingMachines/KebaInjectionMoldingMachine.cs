﻿using System.Timers;
using Timer = System.Timers.Timer;

namespace InjectionMoldingMachineDataAcquisitionService.Domain.InjectionMoldingMachines;
public class KebaInjectionMoldingMachine
{
    public bool IsConnected => _opcUaClient.IsConnected;
    public string MachineId { get; set; }

    private readonly OpcUaClient _opcUaClient;
    private readonly CycleDataToCsvAppender _cycleDataToCsvAppender;
    private readonly Timer _reconnectTimer;
    private readonly IBusControl _busControl;
    private readonly CsvLogger _csvLogger;

    private string? moldId;
    private double? configuredCycle;
    private DateTime currentDoorOpenTime;
    private DateTime currentDoorCloseTime;
    private long cycleElapsed;

    public KebaInjectionMoldingMachine(OpcUaClient opcUaClient, CycleDataToCsvAppender cycleDataToCsvAppender, string machineId, IBusControl busControl)
    {
        _opcUaClient = opcUaClient;
        _cycleDataToCsvAppender = cycleDataToCsvAppender;
        this.MachineId = machineId;

        _reconnectTimer = new Timer(10000);
        _reconnectTimer.Elapsed += ReconnectTimerElapsed;
        _busControl = busControl;
        _csvLogger = new CsvLogger();
    }

    public async Task Connect()
    {
        _reconnectTimer.Enabled = false;
        try
        {
            await _opcUaClient.ConnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{MachineId}: Connection failed. {ex.Message}");
            _reconnectTimer.Enabled = true;
            return;
        }

        var subscription = _opcUaClient.Subscribe(1000);

        subscription.AddMonitorItem("ns=4;s=APPL.system.sv_CycleTime_KVB", "CycleTime", 1000, new List<Action<MetricMessage>>() {
            PublishMetricMessage,
            HandleInjectionCycle });
        subscription.AddMonitorItem("ns=4;s=APPL.system.sv_CycleTime", "CycleTime", 1000, new List<Action<MetricMessage>>() {
            HandleRawInjectionCycle });
        subscription.AddMonitorItem("ns=4;s=SYS.IO.ONBOARD.DI:40.value", "DoorOpenned", 1000, new List<Action<MetricMessage>>() {
            PublishMetricMessage,
            HandleDoorOpened });

        subscription.AddMonitorItem("ns=4;s=APPL.system.di_DoorOpenPulse", "DoorOpenPulse", 1000, new List<Action<MetricMessage>>() {PublishMetricMessage});
        subscription.AddMonitorItem("ns=4;s=APPL.system.di_MoldClosed", "MoldClosed", 1000, new List<Action<MetricMessage>>() {PublishMetricMessage});
        subscription.AddMonitorItem("ns=4;s=APPL.system.di_SafetyDoorMid", "SafetyDoor", 1000, new List<Action<MetricMessage>>() {PublishMetricMessage});
        subscription.AddMonitorItem("ns=4;s=APPL.EasyNet.sv_iShotCounter", "ShotCount", 1000, new List<Action<MetricMessage>>() {PublishMetricMessage});
    }

    public void SetMoldId(string moldId)
    {
        this.moldId = moldId;
    }

    public void SetCycle(double configuredCycle)
    {
        this.configuredCycle = configuredCycle;
    }

    private void PublishMetricMessage(MetricMessage metricMessage)
    {
        _busControl.Publish(new UaMessage(metricMessage.Name, metricMessage.Value));
        _csvLogger.Log(MachineId, metricMessage.Name, metricMessage.Value, metricMessage.Timestamp);
    }

    private void HandleDoorOpened(MetricMessage metricMessage)
    {
        var timestamp = metricMessage.Timestamp;
        bool doorOpened = !((byte)metricMessage.Value == 0);
        if (doorOpened)
        {
            currentDoorOpenTime = timestamp;
        }
        else
        {
            currentDoorCloseTime = timestamp;
        }
    }

    public void HandleInjectionCycle(MetricMessage metricMessage)
    {
        var timestamp = metricMessage.Timestamp;
        var cycle = (long)metricMessage.Value;

        var openTime = currentDoorCloseTime.Subtract(currentDoorOpenTime);
        var cycleTime = TimeSpan.FromTicks(cycle * 10);

        _cycleDataToCsvAppender.AppendData(timestamp, cycleTime, openTime, moldId, configuredCycle);
    }

    public void HandleRawInjectionCycle(MetricMessage metricMessage)
    {
        var timestamp = metricMessage.Timestamp;
        var cycle = (long)metricMessage.Value;

        if (cycle < cycleElapsed)
        {
            _csvLogger.Log(MachineId, "RawCycle", cycleElapsed, timestamp);
        }
        cycleElapsed = cycle;
    }

    private async void ReconnectTimerElapsed(object? sender, ElapsedEventArgs args)
    {
        if (!IsConnected)
        {
            await Connect();
        }
        else
        {
            _reconnectTimer.Enabled = false;
        }
    }
}
