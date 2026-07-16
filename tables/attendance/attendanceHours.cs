using Microsoft.Xrm.Sdk;
using System;
using AttendanceManagement.Schema;

namespace AttendanceManagement.Plugins
{
    public class attendanceHours : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var target = (Entity)((IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext)))
                .InputParameters["Target"];

            if (!target.Contains(Attendance.CheckInTime) ||
                !target.Contains(Attendance.CheckOutTime))
                return;

            var checkIn = target.GetAttributeValue<DateTime>(Attendance.CheckInTime);
            var checkOut = target.GetAttributeValue<DateTime>(Attendance.CheckOutTime);

            if (checkOut <= checkIn)
                throw new InvalidPluginExecutionException(
                    "Check Out Time must be later than Check In Time.");

            target[Attendance.WorkHours] =
                Math.Round((decimal)(checkOut - checkIn).TotalHours, 2);
        }
    }
}