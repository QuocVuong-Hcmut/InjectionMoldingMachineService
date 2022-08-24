using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitaniumAS.Opc.Client.Da;

namespace InjectionMoldingMachineDataAcquisitionService.Communication.Clients
{
    public class OpcDaSubcription
    {
        private OpcDaServer _opcDaServer;
        private OpcDaItemDefinition[] opcDaItems = new OpcDaItemDefinition[1];
        public OpcDaGroup opcDaGroup { set; get; }
        private readonly List<OpcDaNotificationHandler> _notificationHandlers = new( );
        public OpcDaSubcription (OpcDaServer opcDaServer)
        {
            _opcDaServer=opcDaServer;

        }
        public void AddMonitorItem (string nodeId,bool isActive,List<Action<MetricMessage>> handler)
        {
            opcDaGroup=_opcDaServer.AddGroup(nodeId);
            opcDaGroup.KeepAlive=TimeSpan.FromMilliseconds(5000);
            opcDaGroup.IsActive=isActive;
            opcDaGroup.IsSubscribed=true;
            var Item = new OpcDaItemDefinition
            {
                ItemId=nodeId,
                IsActive=true
            };
           // var listTempt = opcDaItems.Append(Item);
            opcDaItems[0]=Item;
            OpcDaItemResult[] results = opcDaGroup.AddItems(opcDaItems);
            foreach ( OpcDaItemResult result in results )
            {
                if ( result.Error.Failed ) ;
            }
            var notificationHandler = new OpcDaNotificationHandler(handler,opcDaGroup);
            //opcDaGroup.ValuesChanged+=OpcDaGroup_ValuesChanged;
        }


    }
    public class OpcDaNotificationHandler
    {
        private event Action<MetricMessage>? _messageHandlers;
        public OpcDaGroup OpcDaGroup { get; private set; }

        public OpcDaNotificationHandler (List<Action<MetricMessage>> messageHandler,OpcDaGroup opcDaGroup)
        {
            messageHandler.ForEach(h => _messageHandlers+=h);
            OpcDaGroup=opcDaGroup;
            OpcDaGroup.ValuesChanged+=HandleNotification;
        }

        public void HandleNotification (object? sender,OpcDaItemValuesChangedEventArgs e)
        {
            try
            {
                   if ( e.Values[0] is not null )
                 {
                var Timestamp = new DateTime(e.Values[0].Timestamp.Year,e.Values[0].Timestamp.Month,e.Values[0].Timestamp.Day,e.Values[0].Timestamp.Hour,e.Values[0].Timestamp.Minute,e.Values[0].Timestamp.Second,e.Values[0].Timestamp.Millisecond);
                    _messageHandlers?.Invoke(new MetricMessage((sender as OpcDaGroup).Name,e.Values[0].Value,Timestamp));
               }

            }
            catch {
            
            }

        }

    }
}
