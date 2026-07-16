using System;
using Microsoft.Xrm.Sdk;

namespace AttendanceManagement.Plugins
{
    public class experienceYearsAutoCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            ITracingService trace =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            if (context.Depth > 1) return;

            if (!context.InputParameters.Contains("Target"))
            {
                trace.Trace("Target not found");
                return;
            }

            Entity employee =
                (Entity)context.InputParameters["Target"];

            try
            {
                if (!employee.Contains("oma_hiredate"))
                {
                    trace.Trace("Hire Date not found");
                    return;
                }

                DateTime hireDate =
                    employee.GetAttributeValue<DateTime>(
                        "oma_hiredate");

                DateTime today = DateTime.Today;

                decimal experienceYears =
                    Math.Round(
                        (decimal)(today - hireDate).TotalDays / 365,
                        2
                    );

                employee["oma_experienceyears"] =
                    experienceYears;

                trace.Trace(
                    $"Experience Calculated: {experienceYears} Years");
            }
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.ToString());
                throw;
            }
        }
    }
}