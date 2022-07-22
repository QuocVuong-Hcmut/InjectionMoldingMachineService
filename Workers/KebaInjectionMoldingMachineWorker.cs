namespace InjectionMoldingMachineDataAcquisitionService.Workers;
public class KebaInjectionMoldingMachineWorker : BackgroundService
{
    private readonly List<KebaInjectionMoldingMachine> _machines = new();
    private readonly KebaConfigurationMessageObserver _configurationMessageObserver;

    public KebaInjectionMoldingMachineWorker(List<KebaInjectionMoldingMachineConfiguration> configurations, string folderPath, IBusControl busControl, KebaConfigurationMessageObserver configurationMessageObserver)
    {
        foreach (var configuration in configurations)
        {
            var appender = new CycleDataToCsvAppender(folderPath, configuration.MachineName);
            var opcUaClient = new OpcUaClient(configuration.Url);
            var machine = new KebaInjectionMoldingMachine(opcUaClient, appender, configuration.MachineName, busControl);

            _machines.Add(machine);
        }
        _configurationMessageObserver = configurationMessageObserver;
        _configurationMessageObserver.ConfigurationMessageReceived += OnReceiveConfigurationMessage;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var machine in _machines)
        {
            await machine.Connect();
        }
    }

    private void OnReceiveConfigurationMessage(ConfigurationMessage message)
    {
        var machine = _machines.FirstOrDefault(m => m.MachineId == message.MachineId);

        if (machine is not null)
        {
            machine.SetCycle(message.CycleTime);
            machine.SetMoldId(message.MoldId);
        }
    }
}
