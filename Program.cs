#pragma warning disable CS8604 // Possible null reference argument.
using InjectionMoldingMachineDataAcquisitionService.Communication.Consumers;
using InjectionMoldingMachineDataAcquisitionService.Jobs;
using InjectionMoldingMachineDataAcquisitionService.Workers;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName="Injection Molding Machine Service";
    })
    .ConfigureServices((builder,services) =>
    {
        var config = builder.Configuration;

        services.Configure<MqttOptions>(config.GetSection("MqttOptions"));
        var mqtt = config.GetSection("MqttOptions").Get<MqttOptions>( );
        services.AddSingleton<MqttClient>( );
        services.AddSingleton<KebaConfigurationMessageObserver>( );
        services.AddMassTransit(x =>
        {
            x.UsingGrpc((context,cfg) =>
            {
                cfg.Host(h =>
                {
                    h.Host="127.0.0.1";
                    h.Port=8181;
                });
                cfg.ReceiveEndpoint("send-config",e =>
                {
                    var serviceProvider = services.BuildServiceProvider( );
                    e.Consumer(( ) => new ConfigurationMessageConsumer(serviceProvider.GetRequiredService<MqttClient>( )));
                    e.Consumer(( ) => new CommandMessageConsumer(serviceProvider.GetRequiredService<MqttClient>( )));
                });

                cfg.ReceiveEndpoint("send-config-keba",e =>
                {
                    var serviceProvider = services.BuildServiceProvider( );
                    e.Consumer(( ) => new KebaConfigurationMessageConsumer(serviceProvider.GetRequiredService<KebaConfigurationMessageObserver>( )));
                });
            });
        });

        //services.AddHostedService<KebaInjectionMoldingMachineWorker>(s =>
        //{
        //    var machines = config.GetSection("KebaInjectionMoldingMachines").Get<KebaInjectionMoldingMachineConfiguration[]>( );

        //    var worker = new KebaInjectionMoldingMachineWorker(machines.ToList( ),@"D:\test data",s.GetService<IBusControl>( ),s.GetService<KebaConfigurationMessageObserver>( ));

        //    return worker;
        //});
        services.AddHostedService<OpcDaInjectionMoldingMachineWorker>(s =>
        {
            var machines = config.GetSection("OpcDaInjectionMoldingMachines").Get<OpcDaInjectionMoldingMachineConfiguration[]>( );

            var worker = new OpcDaInjectionMoldingMachineWorker(machines.ToList( ),@"D:\test data",s.GetService<IBusControl>( ),s.GetService<KebaConfigurationMessageObserver>( ));

            return worker;
        });
        //services.AddHostedService<IotBoardInjectionMoldingMachineWorker>( );


        //    services.AddQuartz(q =>
        //    {
        //        FileServerConfiguration[] fileServers = config.GetSection("FileServers").Get<FileServerConfiguration[]>();

        //        foreach (var fileServer in fileServers)
        //        {
        //            var jobConfigTable = new Dictionary<string, object>()
        //            {
        //                { "Host", fileServer.Host },
        //                { "MachineName", fileServer.MachineName }
        //            };
        //            JobDataMap map = new((IDictionary<string, object>)jobConfigTable);

        //            q.AddJob<CycleFileDownloadingJob>(j =>
        //            {
        //                j.WithIdentity($"{fileServer.MachineName}C")
        //                 .UsingJobData(map)
        //                 .Build();
        //            });
        //            q.AddJob<StatusFileDownloadingJob>(j =>
        //            {
        //                j.WithIdentity($"{fileServer.MachineName}S")
        //                 .UsingJobData(map)
        //                 .Build();
        //            });

        //            q.AddTrigger(t =>
        //            {
        //                t.StartNow()
        //                 .WithCronSchedule(CronScheduleBuilder
        //                 .DailyAtHourAndMinute(18, 44))
        //                 .ForJob($"{fileServer.MachineName}C");
        //            });
        //            q.AddTrigger(t =>
        //            {
        //                t.StartNow()
        //                 .WithCronSchedule(CronScheduleBuilder
        //                 .DailyAtHourAndMinute(18, 44))
        //                 .ForJob($"{fileServer.MachineName}S");
        //            });

        //            q.AddTrigger(t =>
        //            {
        //                t.StartNow()
        //                 .WithCronSchedule(CronScheduleBuilder
        //                 .DailyAtHourAndMinute(6, 44))
        //                 .ForJob($"{fileServer.MachineName}C");
        //            });
        //            q.AddTrigger(t =>
        //            {
        //                t.StartNow()
        //                 .WithCronSchedule(CronScheduleBuilder
        //                 .DailyAtHourAndMinute(6, 44))
        //                 .ForJob($"{fileServer.MachineName}S");
        //            });
        //        }
        //        q.AddJobListener<JobFailureHandler>();
        //    });

        //    services.AddQuartzHostedService(options =>
        //    {
        //        options.WaitForJobsToComplete = true;
        //    });
    })
    .Build( );

await host.RunAsync( );
#pragma warning restore CS8604 // Possible null reference argument.