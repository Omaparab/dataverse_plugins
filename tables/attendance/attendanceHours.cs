using Microsoft.Xrm.Sdk;
using System;

namespace AttendanceManagement.Plugins
{
    public class attendanceHours : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var target = (Entity)((IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext)))
                .InputParameters["Target"];

            if (!target.Contains("oma_checkintime") ||
                !target.Contains("oma_checkouttime"))
                return;

            var checkIn = target.GetAttributeValue<DateTime>("oma_checkintime");
            var checkOut = target.GetAttributeValue<DateTime>("oma_checkouttime");

            if (checkOut <= checkIn)
                throw new InvalidPluginExecutionException(
                    "Check Out Time must be later than Check In Time.");

            target["oma_workhours"] =
                Math.Round((decimal)(checkOut - checkIn).TotalHours, 2);
        }
    }
}