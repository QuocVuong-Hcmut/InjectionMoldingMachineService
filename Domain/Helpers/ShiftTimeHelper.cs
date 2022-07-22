namespace InjectionMoldingMachineDataAcquisitionService.Domain.Helpers;
public static class ShiftTimeHelper
{
    public static TimeSpan DayShiftStartTime = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(45));
    public static TimeSpan DayShiftEndTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(45));

    public static bool IsDayShift(DateTime time)
    {
        var timeOfDay = time.TimeOfDay;

        if (timeOfDay > DayShiftStartTime && timeOfDay < DayShiftEndTime)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsYesterdayShift(DateTime time)
    {
        var timeOfDay = time.TimeOfDay;

        if (timeOfDay < DayShiftStartTime)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
