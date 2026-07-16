using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using AttendanceManagement.Schema;

namespace AttendanceManagement.Plugins
{
    public class preventDepartmentDeletion : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService organizationService = factory.CreateOrganizationService(context.UserId);
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 1) return;

            if (!context.InputParameters.Contains("Target"))
            {
                trace.Trace("Target not found");
                return;
            }


            EntityReference departmentRef = (EntityReference)context.InputParameters["Target"];

            try
            {
                QueryExpression query = new QueryExpression(Employee.EntityName);

                query.Criteria.AddCondition(
                    Employee.Department,
                    ConditionOperator.Equal,
                    departmentRef.Id
                );

                EntityCollection entityCollection = organizationService.RetrieveMultiple(query);

                if (entityCollection.Entities.Count > 0)
                {
                    trace.Trace(
                        "Employees associated with department: "
                        + entityCollection.Entities.Count);

                    throw new InvalidPluginExecutionException(
                        "Department cannot be deleted because employees are associated with it.");
                }

                trace.Trace("Department can be deleted.");
            }
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.Message);
                throw;
            }
        }
    }
}