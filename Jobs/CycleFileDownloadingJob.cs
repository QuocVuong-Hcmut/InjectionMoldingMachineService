namespace InjectionMoldingMachineDataAcquisitionService.Jobs;
public class CycleFileDownloadingJob : FileDownloadingJob
{
    protected override string GetFileName()
    {
        return ShiftDataFileNameHelper.GetCurrentShiftCycleFileName();
    }
}
