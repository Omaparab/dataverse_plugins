using AttendanceManagement.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;

namespace AttendanceManagement.Plugins
{
    /// <summary>
    /// Prevents deletion of a Department when employees are associated with it.
    /// Checks for existing employee records linked to the department and blocks
    /// deletion if active employees are found.
    /// </summary>
    /// <history>
    /// 17.07.2026 - Omkar Parab: Created
    /// </history>
    /// <remarks>
    /// Registration:
    ///     On Delete of Department, Pre Validation, Sync
    /// </remarks>


    public class preventDepartmentDeletion : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 1) return;

            if (!context.InputParameters.Contains("Target"))
            {
                trace.Trace("Target not found");
                return;
            }


            
            try
            {
                Implementation(context, trace, service);
            }
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.Message);
                throw;
            }
        }

        public void Implementation(IPluginExecutionContext context, ITracingService trace, IOrganizationService service)
        {
            EntityReference departmentRef = (EntityReference)context.InputParameters["Target"];

            QueryExpression query = new QueryExpression(Employee.EntityName);

            query.Criteria.AddCondition(
                Employee.Department,
                ConditionOperator.Equal,
                departmentRef.Id
            );

            EntityCollection entityCollection = service.RetrieveMultiple(query);

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
    }
}