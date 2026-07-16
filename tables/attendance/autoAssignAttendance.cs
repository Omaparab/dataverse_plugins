using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagement.Plugins
{
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

            Entity attendance = (Entity)context.InputParameters["Target"];

            if (!attendance.Contains("oma_employee"))
            {
                trace.Trace("Employee field not found");
                return;
            }

            try
            {
                EntityReference empRef = attendance.GetAttributeValue<EntityReference>("oma_employee");

                if (empRef == null)
                {
                    trace.Trace("Employee reference is null");
                    return;
                }

                Entity employee = organizationService.Retrieve(
                    "oma_employee",
                    empRef.Id,
                    new ColumnSet("oma_department", "oma_shift")
                );

                EntityReference department = employee.GetAttributeValue<EntityReference>("oma_department");
                EntityReference shift = employee.GetAttributeValue<EntityReference>("oma_shift");

                Entity attendanceUpdate = new Entity(attendance.LogicalName);
                attendanceUpdate.Id = attendance.Id;

                bool updateRequired = false;

                if (department != null)
                {
                    attendanceUpdate["oma_department"] = department;

                    trace.Trace(
                        $"Department assigned: {department.Name}"
                    );

                    updateRequired = true;
                }

                if (shift != null)
                {
                    attendanceUpdate["oma_shift"] = shift;

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
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.ToString());
                throw new InvalidPluginExecutionException(e.Message, e);
            }
        }
    }
}