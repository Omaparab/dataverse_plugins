using System;
using Microsoft.Xrm.Sdk;
using AttendanceManagement.Schema;

namespace AttendanceManagement.Plugins
{
    public class experienceYearsAutoCalculation : IPlugin
    {
        /// <summary>
        /// Automatically calculates employee experience based on the Hire Date
        /// and current system date. The calculated experience value is populated
        /// before the Employee record is saved.
        /// </summary>
        /// <history>
        /// 17.07.2026 - Omkar Parab: Created
        /// </history>
        /// <remarks>
        /// Registration:
        ///     On Create of Employee (Emp), Pre Operation, Sync
        /// </remarks>

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 1) return;

            if (!context.InputParameters.Contains("Target"))
            {
                trace.Trace("Target not found");
                return;
            }

           

            try
            {
                Implementation(context, trace);
            }
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.ToString());
                throw;
            }
        }


        public void Implementation(IPluginExecutionContext context, ITracingService trace)
        {
            Entity employee = (Entity)context.InputParameters["Target"];

            if (!employee.Contains(Employee.HireDate))
            {
                trace.Trace("Hire Date not found");
                return;
            }

            DateTime hireDate =
                employee.GetAttributeValue<DateTime>(
                    Employee.HireDate);

            DateTime today = DateTime.Today;

            decimal experienceYears =
                Math.Round(
                    (decimal)(today - hireDate).TotalDays / 365,
                    2
                );

            employee[Employee.ExperienceYears] =
                experienceYears;

            trace.Trace(
                $"Experience Calculated: {experienceYears} Years");
        }
    }
}