using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionMoldingMachineDataAcquisitionService.Domain.Helpers;
public static class ShiftDataFileNameHelper
{
    public static string GetCurrentShiftCycleFileName()
    {
        return $"C{GetCurrentShiftFileName()}";
    }
    
    public static string GetCurrentShiftStatusFileName()
    {
        return $"S{GetCurrentShiftFileName()}";
    }

    private static string GetCurrentShiftFileName()
    {
        var now = DateTime.Now;
        var today = now.Date;

        if (ShiftTimeHelper.IsDayShift(now))
        {
            return $"1{today:ddMMyy}.csv";
        }
        else
        {
            if (ShiftTimeHelper.IsYesterdayShift(now))
            {
                return $"2{today.AddDays(-1):ddMMyy}.csv";
            }
            else
            {
                return $"2{today:ddMMyy}.csv";
            }
        }
    }
}
