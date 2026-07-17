using AttendanceManagement.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagement.Plugins
{
    /// <summary>
    /// Automatically retrieves and populates related Department and Shift
    /// information from the selected Employee record when a new Attendance
    /// record is created.
    /// </summary>
    /// <history>
    /// 17.07.2026 - Omkar Parab: Created
    /// </history>
    /// <remarks>
    /// Registration:
    ///     On Create of Attendance, Post Operation, Sync
    /// </remarks>

    public class autoAssignAttendance : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService organizationService = factory.CreateOrganizationService(context.UserId);
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 1) return;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
            {
                trace.Trace("Target not found");
                return;
            }     

            try
            {
                Implementation(context, trace, organizationService);
            }
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.ToString());
                throw new InvalidPluginExecutionException(e.Message, e);
            }
        }

        public void Implementation(IPluginExecutionContext context, ITracingService trace, IOrganizationService organizationService)
        {
            Entity attendance = (Entity)context.InputParameters["Target"];

            if (!attendance.Contains(Attendance.Employee))
            {
                trace.Trace("Employee field not found");
                return;
            }

            EntityReference empRef = attendance.GetAttributeValue<EntityReference>(Attendance.Employee);

            if (empRef == null)
            {
                trace.Trace("Employee reference is null");
                return;
            }

            Entity employee = organizationService.Retrieve(
                Employee.EntityName,
                empRef.Id,
                new ColumnSet(Employee.Department, Employee.Shift)
            );

            EntityReference department = employee.GetAttributeValue<EntityReference>(Employee.Department);
            EntityReference shift = employee.GetAttributeValue<EntityReference>(Employee.Shift);

            Entity attendanceUpdate = new Entity(attendance.LogicalName);
            attendanceUpdate.Id = attendance.Id;

            bool updateRequired = false;

            if (department != null)
            {
                attendanceUpdate[Attendance.Department] = department;

                trace.Trace(
                    $"Department assigned: {department.Name}"
                );

                updateRequired = true;
            }

            if (shift != null)
            {
                attendanceUpdate[Attendance.Shift] = shift;

                trace.Trace(
                    $"Shift assigned: {shift.Name}"
                );

                updateRequired = true;
            }

            if (updateRequired)
            {
                organizationService.Update(attendanceUpdate);
                trace.Trace("Attendance updated successfully.");

                String AttendanceValid = (String)context.SharedVariables["AttendanceValid"];
                trace.Trace($"{AttendanceValid}");
            }
        }
    }
}