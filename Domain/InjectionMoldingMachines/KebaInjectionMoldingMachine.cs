using System.Timers;
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

    private OpcUaSubscription? _subscription;
    private string? moldId;
    private double? configuredCycle;
    private DateTime currentDoorOpenTime;
    private DateTime currentDoorCloseTime;
    private long cycleElapsed;
    private DateTime cycleElapsedTimestamp;
    private long lastCycle;
    private DateTime lastCycleTimestamp;

    public KebaInjectionMoldingMachine (OpcUaClient opcUaClient,CycleDataToCsvAppender cycleDataToCsvAppender,string machineId,IBusControl busControl)
    {
        _opcUaClient=opcUaClient;
        _cycleDataToCsvAppender=cycleDataToCsvAppender;
        this.MachineId=machineId;
        _reconnectTimer=new Timer(10000);
        _reconnectTimer.Elapsed+=ReconnectTimerElapsed;
        _busControl=busControl;
    }

    public async Task Connect ( )
    {
        _reconnectTimer.Enabled=false;
        try
        {
            await _opcUaClient.ConnectAsync( );
        }
        catch ( Exception ex )
        {
            Console.WriteLine($"{MachineId}: Connection failed. {ex.Message}");
            _reconnectTimer.Enabled=true;
            return;
        }

        var subscription = _opcUaClient.Subscribe(1000);

        subscription.AddMonitorItem("ns=4;s=APPL.system.sv_CycleTime","CycleElapsed",1000,new List<Action<MetricMessage>>( ) {
            HandleInjectionCycleElapsed });
        subscription.AddMonitorItem("ns=4;s=SYS.IO.ONBOARD.DI:40.value","DoorOpenned",1000,new List<Action<MetricMessage>>( ) {
            PublishMetricMessage,
            HandleDoorOpened });

        subscription.AddMonitorItem("ns=4;s=APPL.system.di_DoorOpenPulse","DoorOpenPulse",1000,new List<Action<MetricMessage>>( ) { PublishMetricMessage });
        subscription.AddMonitorItem("ns=4;s=APPL.system.di_MoldClosed","MoldClosed",1000,new List<Action<MetricMessage>>( ) { HandleMoldOpen });
        _subscription?.Dispose( );
        _subscription=subscription;
    }

    public void SetMoldId (string moldId)
    {
        this.moldId=moldId;
    }

    public void SetCycle (double configuredCycle)
    {
        this.configuredCycle=configuredCycle;
    }

    private async void PublishMetricMessage (MetricMessage metricMessage)
    {


        _busControl.Publish<UaMessage>(new UaMessage(metricMessage.Name,metricMessage.Value));
    }

    private void HandleDoorOpened (MetricMessage metricMessage)
    {

        var timestamp = metricMessage.Timestamp;
        bool doorOpened = !((byte)metricMessage.Value==0);
        if ( doorOpened )
        {
            currentDoorOpenTime=timestamp;
        }
        else
        {
            currentDoorCloseTime=timestamp;
            var openTime = currentDoorCloseTime.Subtract(currentDoorOpenTime);
            var cycleTime = TimeSpan.FromTicks(lastCycle*10);
            _cycleDataToCsvAppender.AppendData(lastCycleTimestamp,cycleTime,openTime,moldId,configuredCycle);
        }

    }

    private void HandleInjectionCycleElapsed (MetricMessage metricMessage)
    {
        var cycle = (long)metricMessage.Value;

        if ( cycle<cycleElapsed )
        {
            _subscription?.SuspendMonitoredItemSubscription("CycleElapsed");
            lastCycle=cycleElapsed;
            lastCycleTimestamp=cycleElapsedTimestamp;
        }
        cycleElapsed=cycle;
        cycleElapsedTimestamp=metricMessage.Timestamp;
    }

    private void HandleMoldOpen (MetricMessage metricMessage)
    {
        var moldOpened = !(bool)metricMessage.Value;
        if ( moldOpened )
        {
            _subscription?.ContinueMonitoredItemSubscription("CycleElapsed");
        }
    }

    private async void ReconnectTimerElapsed (object? sender,ElapsedEventArgs args)
    {
        if ( !IsConnected )
        {
            await Connect( );
        }
        else
        {
            _reconnectTimer.Enabled=false;
        }
    }
}
