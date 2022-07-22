namespace InjectionMoldingMachineDataAcquisitionService.Domain.Services;
public class CycleDataToCsvAppender
{
    private readonly string folderPath;
    private int shotCount;

    public CycleDataToCsvAppender(string folderPath, string machineId)
    {
        this.folderPath = $"{folderPath}/{machineId}";

        shotCount = GetLastShotCount();
    }

    public void AppendData(DateTime timestamp, TimeSpan cycle, TimeSpan openTime, string? moldId, double? configuredCycle)
    {
        var fileName = ShiftDataFileNameHelper.GetCurrentShiftCycleFileName();
        var filePath = $"{folderPath}/{fileName}";

        CreateFileIfNotExist(filePath);

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.Write($"{timestamp:yyyy-MM-ddTHH:mm:ss},{cycle.TotalSeconds},{openTime.TotalSeconds},{shotCount}");

            if (moldId != null)
            {
                sw.Write($",{moldId}");
            }
            else
            {
                sw.Write(",NotConfigured");
            }

            if (configuredCycle != null)
            {
                sw.Write($",{configuredCycle}");
            }
            else
            {
                sw.Write(",NotConfigured");
            }

            sw.WriteLine();
        }

        shotCount++;
    }

    private int GetLastShotCount()
    {
        var fileName = ShiftDataFileNameHelper.GetCurrentShiftCycleFileName();
        var filePath = $"{this.folderPath}/{fileName}";

        if (File.Exists(filePath))
        {
            var lastLine = File.ReadLines(filePath).Last();
            var lastLineCells = lastLine.Split(",");
            return Convert.ToInt32(lastLineCells[3]) + 1;
        }
        else
        {
            return 0;
        }
    }

    private void CreateFileIfNotExist(string filePath)
    {
        if (!File.Exists(filePath))
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("Timestamp,CycleTime,OpenTime,CounterShot,MoldId,SetCycle");
            }

            shotCount = 1;
        }
    }
}
