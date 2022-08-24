using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace InjectionMoldingMachineDataAcquisitionService.Workers;

public class OpcDaInjectionMoldingMachineWorker: BackgroundService
    {
        private readonly List<OpcDaInjectionMoldingMachine> _machines = new( );
        private readonly KebaConfigurationMessageObserver _configurationMessageObserver;

        public OpcDaInjectionMoldingMachineWorker (List<OpcDaInjectionMoldingMachineConfiguration> configurations,
            string folderPath,
            IBusControl busControl,
            KebaConfigurationMessageObserver configurationMessageObserver)
        {
            foreach ( var configuration in configurations )
            {
                var appender = new CycleDataToCsvAppender(folderPath,configuration.MachineName);
                var opcDaClient = new OpcDaClient(configuration.Url);
                var machine = new OpcDaInjectionMoldingMachine(opcDaClient,configuration.MachineName,busControl);
                _machines.Add(machine);
            }
            _configurationMessageObserver=configurationMessageObserver;
            _configurationMessageObserver.ConfigurationMessageReceived+=OnReceiveConfigurationMessage;
        }

        protected async override Task ExecuteAsync (CancellationToken stoppingToken)
        {
            foreach ( var machine in _machines )
            {
                await machine.Connect( );
            }
        }

        private void OnReceiveConfigurationMessage (ConfigurationMessage message)
        {
            var machine = _machines.FirstOrDefault(m => m.MachineId==message.MachineId);

            if ( machine is not null )
            {
                machine.SetCycle(message.CycleTime);
                machine.SetMoldId(message.MoldId);
            }
        }

    }

