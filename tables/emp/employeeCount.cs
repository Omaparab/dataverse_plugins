using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using AttendanceManagement.Schema;

namespace AttendanceManagement.Plugins
{
    public class EmployeeCount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            try
            {
                trace.Trace($"Message:{context.MessageName}");
                trace.Trace($"User:{context.UserId}");
                trace.Trace($"Depth:{context.Depth}");
                trace.Trace($"Parent:{context.ParentContext != null}");

                if (context.Depth > 1) return;

                switch (context.MessageName)
                {
                    case "Create":

                        if (!context.InputParameters.Contains("Target"))
                        {
                            trace.Trace("Target not found");
                            return;
                        }

                        Entity emp1 = (Entity)context.InputParameters["Target"];

                        EntityReference deptRef1 =
                            emp1.GetAttributeValue<EntityReference>(Employee.Department);

                        if (deptRef1 == null)
                        {
                            trace.Trace("Department Reference not found for create employee operation");
                            return;
                        }

                        Entity department1 = service.Retrieve(
                            Department.EntityName,
                            deptRef1.Id,
                            new ColumnSet(Department.EmployeeCount)
                        );

                        int currentCount1 =
                            department1.GetAttributeValue<int?>(Department.EmployeeCount) ?? 0;

                        int newCount1 = currentCount1 + 1;

                        department1[Department.EmployeeCount] = newCount1;

                        service.Update(department1);

                        trace.Trace($"Employee Count Updated : {newCount1}");

                        break;

                    case "Update":

                        if (!context.InputParameters.Contains("Target"))
                        {
                            trace.Trace("Target not found");
                            return;
                        }
                            

                        if (!context.PreEntityImages.Contains("Update Employee Count PreImage"))
                        {
                            trace.Trace("PreImage not found");
                            return;
                        }
                            

                        Entity emp2 = (Entity)context.InputParameters["Target"];
                        Entity preImage2 = context.PreEntityImages["Update Employee Count PreImage"];

                        EntityReference oldDept2 =
                            preImage2.GetAttributeValue<EntityReference>(Employee.Department);

                        if (!emp2.Contains(Employee.Department))
                        {
                            trace.Trace("Department not changed");
                            return;
                        }

                        EntityReference newDept2 =
                            emp2.GetAttributeValue<EntityReference>(Employee.Department);

                        if (oldDept2 == null || newDept2 == null)
                        {
                            trace.Trace("Old or New Department is null");
                            return;
                        }

                        if (oldDept2.Id == newDept2.Id)
                        {
                            trace.Trace("Department not changed");
                            return;
                        }

                        Entity oldDepartment2 = service.Retrieve(
                            Department.EntityName,
                            oldDept2.Id,
                            new ColumnSet(Department.EmployeeCount)
                        );

                        int oldCount2 =
                            oldDepartment2.GetAttributeValue<int?>(Department.EmployeeCount) ?? 0;

                        oldCount2 = Math.Max(0, oldCount2 - 1);

                        oldDepartment2[Department.EmployeeCount] = oldCount2;

                        service.Update(oldDepartment2);

                        trace.Trace($"Old Department Count Updated : {oldCount2}");

                        Entity newDepartment2 = service.Retrieve(
                            Department.EntityName,
                            newDept2.Id,
                            new ColumnSet(Department.EmployeeCount)
                        );

                        int newCount2 =
                            newDepartment2.GetAttributeValue<int?>(Department.EmployeeCount) ?? 0;

                        newCount2 += 1;

                        newDepartment2[Department.EmployeeCount] = newCount2;

                        service.Update(newDepartment2);

                        trace.Trace($"New Department Count Updated : {newCount2}");

                        break;

                    case "Delete":

                        if (!context.PreEntityImages.Contains("Delete Employee Count PreImage"))
                        {
                            trace.Trace("PreImage not found");
                            return;
                        }
                            

                        Entity preImage3 = context.PreEntityImages["Delete Employee Count PreImage"];

                        EntityReference deptRef3 =
                            preImage3.GetAttributeValue<EntityReference>(Employee.Department);

                        if (deptRef3 == null)
                        {
                            trace.Trace("Department Reference not found for delete employee operation");
                            return;
                        }

                        Entity department3 = service.Retrieve(
                            Department.EntityName,
                            deptRef3.Id,
                            new ColumnSet(Department.EmployeeCount)
                        );

                        int currentCount3 =
                            department3.GetAttributeValue<int?>(Department.EmployeeCount) ?? 0;

                        currentCount3 = Math.Max(0, currentCount3 - 1);

                        department3[Department.EmployeeCount] = currentCount3;

                        service.Update(department3);

                        trace.Trace($"Employee Deleted. Department Count Updated : {currentCount3}");

                        break;
                }
            }
            catch (Exception ex)
            {
                trace.Trace($"Error : {ex}");
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}