using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using AttendanceManagement.Schema;

namespace AttendanceManagement.Plugins
{
    /// <summary>
    /// Prevents creation of duplicate attendance records.
    /// Verifies whether an attendance entry already exists for the selected
    /// employee on the same attendance date and blocks record creation if found.
    /// </summary>
    /// <history>
    /// 17.07.2026 - Omkar Parab: Created
    /// </history>
    /// <remarks>
    /// Registration:
    ///     On Create of Attendance, Pre Validation, Sync
    /// </remarks>

    public class attendanceDuplicateCheck : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService organizationService = factory.CreateOrganizationService(context.UserId);
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 1) return;

            if (!context.InputParameters.Contains("Target")){
                trace.Trace("Target not found");
                return;
            }

            
            
            try
            {
                Implementation(context, trace, organizationService);
            }
            catch(Exception e)
            {
                trace.Trace("Exception:" + e.Message);
                throw;
            }

        }

        public void Implementation(IPluginExecutionContext context, ITracingService trace, IOrganizationService organizationService)
        {
            Entity attendanceRecord = (Entity)context.InputParameters["Target"];

            if ((!attendanceRecord.Contains(Attendance.CheckInTime)) || (!attendanceRecord.Contains(Attendance.Employee)))
            {
                trace.Trace("Checkin time or Employee is missing");
                return;
            }

            DateTime checkin = attendanceRecord.GetAttributeValue<DateTime>(Attendance.CheckInTime);
            trace.Trace(checkin.ToString());
            EntityReference name = attendanceRecord.GetAttributeValue<EntityReference>(Attendance.Employee);


            DateTime startdate = checkin.Date;
            DateTime endDate = checkin.AddDays(1).Date;

            QueryExpression query = new QueryExpression(Attendance.EntityName);

            query.Criteria.AddCondition(
                Attendance.Employee,
                ConditionOperator.Equal,
                name.Id
                );

            query.Criteria.AddCondition(
                Attendance.CheckInTime,
                ConditionOperator.GreaterEqual,
                startdate
                );

            query.Criteria.AddCondition(
                Attendance.CheckInTime,
                ConditionOperator.LessThan,
                endDate
                );

            EntityCollection entityCollection = organizationService.RetrieveMultiple(query);
            context.SharedVariables["AttendanceValid"] = $"Attendance is Valid";
            if (entityCollection.Entities.Count > 0)
            {
                trace.Trace("Duplicate Records count:" + entityCollection.Entities.Count);


                throw new InvalidPluginExecutionException($"Duplicate Records Found");
            }
        }
    }
}
