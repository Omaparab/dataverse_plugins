using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AttendanceManagement.Plugins
{
    public class shiftCapacityValidator : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            IOrganizationService organizationService =
                factory.CreateOrganizationService(context.UserId);

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
                if (!employee.Contains("oma_shift"))
                {
                    trace.Trace("Shift not found");
                    return;
                }

                EntityReference shiftRef =
                    employee.GetAttributeValue<EntityReference>(
                        "oma_shift");

                if (shiftRef == null)
                {
                    trace.Trace("Shift reference is null");
                    return;
                }

                QueryExpression query =
                    new QueryExpression("oma_employee");

                query.ColumnSet =
                    new ColumnSet("oma_name");

                query.Criteria.AddCondition(
                    "oma_shift",
                    ConditionOperator.Equal,
                    shiftRef.Id
                );

                EntityCollection employees =
                    organizationService.RetrieveMultiple(query);

                int currentEmployees =
                    employees.Entities.Count;

                const int MAX_CAPACITY = 2;

                trace.Trace(
                    "Current Employees: " + currentEmployees);

                if (currentEmployees >= MAX_CAPACITY)
                {
                    throw new InvalidPluginExecutionException(
                        "Shift capacity exceeded. Maximum 2 employees can be assigned to a shift."
                    );
                }
            }
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.ToString());
                throw;
            }
        }
    }
}