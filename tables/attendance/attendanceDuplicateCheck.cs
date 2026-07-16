using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AttendanceManagement.Plugins
{
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

            Entity attendanceRecord = (Entity)context.InputParameters["Target"];

            try
            {
                if((!attendanceRecord.Contains("oma_checkintime")) || (!attendanceRecord.Contains("oma_employee"))){
                    trace.Trace("Checkin time or Employee is missing");
                    return;
                }

                DateTime checkin = attendanceRecord.GetAttributeValue<DateTime>("oma_checkintime");
                trace.Trace(checkin.ToString());
                EntityReference name = attendanceRecord.GetAttributeValue<EntityReference>("oma_employee");
                

                DateTime startdate = checkin.Date;
                DateTime endDate = checkin.AddDays(1).Date;

                QueryExpression query = new QueryExpression("oma_attendance");

                query.Criteria.AddCondition(
                    "oma_employee",
                    ConditionOperator.Equal,
                    name.Id
                    );

                query.Criteria.AddCondition(
                    "oma_checkintime",
                    ConditionOperator.GreaterEqual,
                    startdate
                    );

                query.Criteria.AddCondition(
                    "oma_checkintime",
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
            catch(Exception e)
            {
                trace.Trace("Exception:" + e.Message);
                throw;
            }

            

            


        }
    }
}
