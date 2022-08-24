using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace InjectionMoldingMachineDataAcquisitionService.Domain.InjectionMoldingMachines
{
    public class OpcDaInjectionMoldingMachine
    {
        private OpcDaClient _opcDaClient;
        public string MachineId;
        private string? moldId;
        private double? configuredCycle;
        private IBusControl _busControl;
        private readonly System.Timers.Timer _reconnectTimer;
        public bool IsConnected => _opcDaClient.IsConnected;
        public OpcDaInjectionMoldingMachine (OpcDaClient opcDaClient, string machineId, IBusControl busControl )
        {
            _opcDaClient = opcDaClient;
            MachineId= machineId;
            _busControl = busControl;
           _reconnectTimer=new (10000);
        }
        public async Task Connect ( )
        {
            _reconnectTimer.Enabled=false;
            try
            {
                await _opcDaClient.ConnectAsync( );
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"{MachineId}: Connection failed. {ex.Message}");
               _reconnectTimer.Enabled=true;
                return;
            }

            var subscription = _opcDaClient.Subscribe();
            subscription.AddMonitorItem("Random.Int2",true,new List<Action<MetricMessage>>( ) { PublishMetricMessage });
           // subscription.AddMonitorItem("Random.Int1",true,new List<Action<MetricMessage>>( ) { HandleMoldOpen });
        }
        //private void HandleMoldOpen (MetricMessage metricMessage)
        //{
        //    var moldOpened = (float)metricMessage.Value;
        //    if ( moldOpened )
        //    {
        //       // Console.WriteLine(moldOpened);
        //    }
        //}
        private async void PublishMetricMessage (MetricMessage metricMessage)
        {
            Console.WriteLine(metricMessage.Value);
            var endpoint = await _busControl.GetSendEndpoint(new Uri("http://127.0.0.1:8181/listen"));
            await endpoint.Send<DaMessage>(new DaMessage
        (
        "kkk",5
        ));
            _busControl.Publish<DaMessage>(new DaMessage(metricMessage.Name,metricMessage.Value));
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
        public void SetMoldId (string moldId)
        {
            this.moldId=moldId;
        }

        public void SetCycle (double configuredCycle)
        {
            this.configuredCycle=configuredCycle;
        }
    }
}
