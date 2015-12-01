using System;
using System.Collections.Generic;
using System.Linq;
using Amaris.Security;
using AmarisGate.Dal;
using Developpez.Dotnet.Collections;

namespace AmarisGate.Controllers
{
    public class ExternalServiceController : BootstrapBaseController
    {
        public void DnaNewOrder(int newEmployeeId)
        {
            //1. Function + Company found(Host By)
            //2. Function only found
            //3. Default package
            Employee newEmployee = DB.Employees.Find(newEmployeeId);

            if (newEmployee == null)
            {
                Response.Write("No employees with that Id were found");
                return;
            }
         
            GATE_MaterialRequest newRequest = new GATE_MaterialRequest
            {
                AmarisOfficeId = (newEmployee.OfficeExtension != null ? newEmployee.OfficeExtension.OfficeId : -1),
                ConcernedEmployeeId = newEmployeeId,
                IsAutomated = true,
                RequestDate = DateTime.Today,
                OrderedByEmployeeId = newEmployee.ManagerId.GetValueOrDefault(),
                OrderStatusId = OrderStatus.HelpDeskProductProposal,
                OrderTypeId = 1
            };

            newRequest.Office = DB.Offices.Find(newRequest.AmarisOfficeId);
            

            //The ids of all the functions the new employee has (1 employee can have several functions)
            //Get functions by using scope
            var employeeFunctions = DB.Employee_Scope
                .Where(sco => sco.EmployeeId == newEmployeeId)
                .Select(sco => sco.FunctionId);

            //List of ids of all the functions each package has
            var packageFunctions =
                from package in DB.Packages
                select new { packageId = package.PackageId, packageFunctionIds = (from aFunction in package.Functions select aFunction.FunctionId) };

            //Find if there is a package for the specific functions of the new Employee
            //Match employee functions with package functions
            List<int> packageIds = new List<int>();
            List<int> functionIds;
            foreach (var tuple in packageFunctions)
            {
                functionIds = (List<int>)tuple.packageFunctionIds;
                //Find if functionIds contains all items inside employeeFunctions
                //http://stackoverflow.com/questions/1520642/does-net-have-a-way-to-check-if-list-a-contains-all-items-in-list-b
                if (!employeeFunctions.Except(functionIds).Any() && employeeFunctions.Any())
                {
                    packageIds.Add(tuple.packageId);
                }
            }

            //There is a package with all the functions of the new Employee. We enter case 1 or 2
            if (!packageIds.IsNullOrEmpty())
            {
                //case 1: Function + Company
                List<Package> possibleTargets = new List<Package>();

                //There is a package more specific for the employee
                if (!possibleTargets.IsNullOrEmpty())
                {
                    //case 1
                    newRequest.PackageId = possibleTargets.First().PackageId; 
                }
                else
                {
                    //case 2
                    newRequest.PackageId = packageIds.First();
                }

                //Request the individual components (aparently this is necessary, although I am not sure why ...)
                Package thePackage = DB.Packages.Find(newRequest.PackageId);

                foreach (var material in thePackage.GenericMaterials)
                {
                    GATE_ComponentRequest component = new GATE_ComponentRequest
                    {
                        MaterialId = material.GenericMaterialId,
                        IsSetup = !material.RequiresSetup,
                        IsDispatched = false,
                        MaterialRequestId = newRequest.MaterialRequestId,
                        StatusId = OrderStatus.HelpDeskProductProposal
                    };

                    GATE_MaterialSuggestion newSuggestion = new GATE_MaterialSuggestion
                    {
                        InStock = false,
                        IsNotStandardProduct = false,
                        Label = string.Empty,
                        CurrencyId = "EUR"
                    };

                    DB.GATE_MaterialSuggestion.Add(newSuggestion);


                    DB.GATE_ComponentRequest.Add(component);
                    component.CurrentSuggestion = newSuggestion;
                }
            }
            //No package has the functions our Employee has, so we skip directly to order the default package
            else
            {
                //case 3
                newRequest.PackageId = null;
            }

            MaterialAction firstAction = new MaterialAction
            {
                ActionId = 26,
                EmployeeId = newEmployee.ManagerId ?? newEmployeeId,
                DetailDate = DateTime.Now,
                Comment  = "Order automatically created via external service"
            };
            newRequest.MaterialAction.Add(firstAction);
            DB.MaterialActions.Add(firstAction);

            //set default Address
            Address defaultAddress = new Address
            {
                Complement = string.Empty,
                CreatedDate = DateTime.Now
            };
            DB.Addresses.Add(defaultAddress);
            newRequest.AddressId = defaultAddress.ID;

            DB.GATE_MaterialRequest.Add(newRequest);

            DB.SaveChanges();
            DB.GATE_MaterialRequest.UpdatePermissions(newRequest.MaterialRequestId);

            MailMan.SendAlertMails(newRequest);

            Response.Write("Request Sucessfull");
        }
    }
}
