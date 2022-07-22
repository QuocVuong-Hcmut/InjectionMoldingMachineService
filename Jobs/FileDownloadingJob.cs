using System.Net;

namespace InjectionMoldingMachineDataAcquisitionService.Jobs;
[PersistJobDataAfterExecution]
public abstract class FileDownloadingJob: IJob
{    
    public async Task Execute(IJobExecutionContext context)
    {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        using WebClient client = new();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        JobDataMap dataMap = context.JobDetail.JobDataMap;
        string? hostUrl = dataMap.GetString("Host");
        string? machineName = dataMap.GetString("MachineName");

        if (String.IsNullOrEmpty(hostUrl) || String.IsNullOrEmpty(machineName))
        {
            return;
        }

        var fileName = GetFileName();
        await client.DownloadFileTaskAsync($"{hostUrl}/{fileName}", @$"D:\test data\{machineName}\{fileName}");
    }

    protected abstract string GetFileName();
}
