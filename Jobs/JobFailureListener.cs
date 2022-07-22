namespace InjectionMoldingMachineDataAcquisitionService.Jobs;
public class JobFailureHandler : IJobListener
{
    public string Name => "FailJobListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken ct)
    {
        if (!context.JobDetail.JobDataMap.Contains(Constants.NumTriesKey))
        {
            context.JobDetail.JobDataMap.Put(Constants.NumTriesKey, 0);
        }

        var numberTries = context.JobDetail.JobDataMap.GetIntValue(Constants.NumTriesKey);
        context.JobDetail.JobDataMap.Put(Constants.NumTriesKey, ++numberTries);

        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken ct)
    {
        if (jobException == null)
        {
            return;
        }

        var numTries = context.JobDetail.JobDataMap.GetIntValue(Constants.NumTriesKey);

        if (numTries > Constants.MaxRetries)
        {
            Console.WriteLine($"Job with ID and type: {0}, {1} has run {2} times and has failed each time.",
                context.JobDetail.Key, context.JobDetail.JobType, Constants.MaxRetries);

            return;
        }

        var trigger = TriggerBuilder
                .Create()
                .WithIdentity(Guid.NewGuid().ToString(), Constants.TriggerGroup)
                .StartAt(DateTime.Now.AddSeconds(Constants.WaitInterval * numTries))
                .Build();

        Console.WriteLine($"Job with ID and type: {0}, {1} has thrown the exception: {2}. Running again in {3} seconds.",
            context.JobDetail.Key, context.JobDetail.JobType, jobException, Constants.WaitInterval * numTries);

        await context.Scheduler.RescheduleJob(context.Trigger.Key, trigger);
    }

    public class Constants
    {
        public static readonly int WaitInterval = 60;
        public static readonly int MaxRetries = 10;
        public static readonly string NumTriesKey = "numTriesKey"; 
        public static readonly string TriggerGroup = "failTriggerGroup";
    }    
}