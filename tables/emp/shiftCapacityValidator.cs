using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using AttendanceManagement.Schema;

namespace AttendanceManagement.Plugins
{
    public class shiftCapacityValidator : IPlugin
    {
        /// <summary>
        /// Validates shift capacity before assigning an employee to a shift.
        /// Checks the current number of employees assigned to the selected shift
        /// and prevents the assignment when the maximum capacity has been reached.
        /// </summary>
        /// <history>
        /// 17.07.2026 - Omkar Parab: Created
        /// </history>
        /// <remarks>
        /// Registration:
        ///     On Create of Employee Shift, Pre Validation, Sync
        /// </remarks>

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


            try
            {
                Implementation(context, trace, organizationService);
            }
            catch (Exception e)
            {
                trace.Trace("Exception: " + e.ToString());
                throw;
            }
        }


        public void Implementation(IPluginExecutionContext context, ITracingService trace, IOrganizationService organizationService)
        {
            Entity employee = (Entity)context.InputParameters["Target"];

            if (!employee.Contains(Employee.Shift))
            {
                trace.Trace("Shift not found");
                return;
            }
            
            EntityReference shiftRef =
                employee.GetAttributeValue<EntityReference>(
                    Employee.Shift);

            if (shiftRef == null)
            {
                trace.Trace("Shift reference is null");
                return;
            }

            QueryExpression query = new QueryExpression(Employee.EntityName);

            query.ColumnSet = new ColumnSet(Employee.Name);

            query.Criteria.AddCondition(
                Employee.Shift,
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
    }
}