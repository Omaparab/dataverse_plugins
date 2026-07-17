using AttendanceManagement.Schema;
using Microsoft.Xrm.Sdk;
using System;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;

namespace AttendanceManagement.Plugins
{
    public class attendanceHours : IPlugin
    {
        /// <summary>
        /// Validates Check-In and Check-Out time values before saving the record.
        /// Ensures Check-Out Time is later than Check-In Time and automatically
        /// calculates total work hours to populate the Work Hours field.
        /// </summary>
        /// <history>
        /// 17.07.2026 - Omkar Parab: Created
        /// </history>
        /// <remarks>
        /// Registration:
        ///     On Create of Attendance, Pre Operation, Sync
        /// </remarks>

        public void Execute(IServiceProvider serviceProvider)
        {

            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                Implementation(serviceProvider);
            }
            catch (Exception e)
            {
                trace.Trace("Exception:" + e.Message);
                throw;
            }
        }

        public void Implementation(IServiceProvider serviceProvider)
        {
            var target = (Entity)((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext))).InputParameters["Target"];

            if (!target.Contains(Attendance.CheckInTime) || !target.Contains(Attendance.CheckOutTime))
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