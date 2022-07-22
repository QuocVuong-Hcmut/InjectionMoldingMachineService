namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages;
public class SynchronizeTimeMessage
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Min { get; set; }
    public int Sec { get; set; }

    public SynchronizeTimeMessage(int year, int month, int day, int hour, int min, int sec)
    {
        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
        Min = min;
        Sec = sec;
    }
}
