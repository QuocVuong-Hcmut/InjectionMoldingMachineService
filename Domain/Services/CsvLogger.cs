namespace InjectionMoldingMachineDataAcquisitionService.Domain.Services;
public class CsvLogger
{
    private readonly string folderPath = @"D:\test data";

    public void Log(string machineId, string name, object value, DateTime timestamp)
    {
        var fileName = ShiftDataFileNameHelper.GetCurrentShiftLogFileName();
        var filePath = $"{folderPath}/{fileName}";

        CreateFileIfNotExist(filePath);

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine($"{machineId},{name},{value},{timestamp:yyyy-MM-ddTHH:mm:ss}");
        }
    }

    private void CreateFileIfNotExist(string filePath)
    {
        if (!File.Exists(filePath))
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("Machine,Name,Value,Timestamp");
            }
        }
    }
}
