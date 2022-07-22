namespace InjectionMoldingMachineDataAcquisitionService.Jobs;
public class StatusFileDownloadingJob: FileDownloadingJob
{
    protected override string GetFileName()
    {
        return ShiftDataFileNameHelper.GetCurrentShiftStatusFileName();
    }
}
