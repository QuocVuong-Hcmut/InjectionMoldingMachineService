using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da;

namespace InjectionMoldingMachineDataAcquisitionService.Communication.Clients
{
    public class OpcDaClient
    {
        private string serverUrl;
        private int keepAliveInterval = 5000;
        private Uri uri;
        private OpcDaServer opcDaServer;
        public bool IsConnected { set; get; }
        public OpcDaClient (string url )
        {
   
            uri =UrlBuilder.Build(url);
            opcDaServer=new(url);
            
        }
        public async Task ConnectAsync ( )
        {
            opcDaServer.Connect ( );
            IsConnected=true;

        }
        public OpcDaSubcription Subscribe ( )
        {
            
            return new OpcDaSubcription(opcDaServer);
        }

    }
}
