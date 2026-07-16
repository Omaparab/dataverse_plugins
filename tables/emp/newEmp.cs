using System;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using AttendanceManagement.Schema;



namespace AttendanceManagement.Plugins
{
    public class newEmp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {


            var tracingService = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("EmployeePlugin started.");

            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if(context.MessageName == "Create" || context.MessageName == "Update")
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