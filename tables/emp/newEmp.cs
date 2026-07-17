using AttendanceManagement.Schema;
using Microsoft.Xrm.Sdk;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;



namespace AttendanceManagement.Plugins
{
    /// <summary>
    /// Validates Employee Name before saving the record.
    /// Only alphabets and spaces are allowed in the Employee Name field.
    /// If invalid characters are detected, record creation or update is prevented.
    /// </summary>
    /// <history>
    /// 17.07.2026 - Omkar Parab: Created
    /// </history>
    /// <remarks>
    /// Registration:
    ///     On Create and Update of Employee (Emp), Pre Operation, Sync
    /// </remarks>

    public class newEmp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("EmployeePlugin started.");

            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                Implementation(context, tracingService);
            }
            catch (Exception e)
            {
                tracingService.Trace("Exception: " + e.ToString());
                throw;
            }


        }


        public void Implementation(IPluginExecutionContext context, ITracingService tracingService)
        {

            if (context.MessageName == "Create" || context.MessageName == "Update")
            {
                var target = (Entity)context.InputParameters["Target"];

                if (target.Contains(Employee.Name))
                {
                    string name = target.GetAttributeValue<string>(Employee.Name);

                    tracingService.Trace($"Validating Name: {name}");

                    if (!Regex.IsMatch(name, @"^[A-Za-z\s]+$"))
                    {
                        throw new InvalidPluginExecutionException(
                            "Name can contain only alphabets and spaces."
                        );
                    }
                }
            }
        }
    }
}