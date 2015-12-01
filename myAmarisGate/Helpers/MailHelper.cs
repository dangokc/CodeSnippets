using Amaris.Mails;
using AmarisGate.Controllers;
using AmarisGate.Dal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AmarisGate.Helpers
{
    public class MailHelper
    {
        private BootstrapBaseController _controller { get; set; }
        private Entities _controllerDb { get; set; }
        private int _senderId { get; set; }
        private string _appName { get; set; }

        public MailHelper(BootstrapBaseController theController, Entities aDb, int aSenderId, string anAppName)
        {
            _controller = theController;
            _controllerDb = aDb;
            _senderId = aSenderId;
            _appName = anAppName;
        }

        /// <summary>
        /// This function acts as a distributor. It choses the right email to send in relation of the status or last action of the material request. 
        /// So, on the process methods this needs to be called whenever an email is needed.
        /// </summary>
        /// <param name="materialRequest">Material Request object</param>
        public void SendAlertMails(GATE_MaterialRequest materialRequest)
        {
            Employee concernedEmployee = materialRequest.ConcernedEmployee;
            Employee orderedEmployee = materialRequest.OrderedByEmployee;
            Employee concernedEmployeeManager = materialRequest.ConcernedEmployee.Manager;
            Employee lom = (materialRequest.Office != null ? materialRequest.Office.LocalOfficeManager.FirstOrDefault() : null);

            string concernedEmployeeName = concernedEmployee.Name;
            string concernedEmployeeEmail = concernedEmployee.Email ?? string.Empty;
            string orderedByEmployeeEmail = (orderedEmployee != null ? orderedEmployee.Email : string.Empty );
            int orderNumber = materialRequest.MaterialRequestId;
            string managerName = concernedEmployeeManager.Name;
            string managerEmail = concernedEmployeeManager != null ? (concernedEmployeeManager.Email ?? string.Empty) : string.Empty;
            string lomEmail = (lom != null ? lom.Email : string.Empty);

            string directorEmails = string.Empty;
            if (concernedEmployee.WorkingForCompany != null)
            {
               directorEmails = String.Join(", ", concernedEmployee.WorkingForCompany.CompanyDirectors.ToDictionary(dir => dir.Email).Keys);
            }
            
            string concernedScopes = string.Empty;
            List<Employee_Scope> concernedScopesList = concernedEmployee.Employee_Scope.ToList();
            if (concernedScopesList.Any())
            {
                foreach (Employee_Scope aScope in concernedScopesList)
                {
                    concernedScopes += "Company: " + aScope.Company.CompanyCodeName + ", Function: " + aScope.Employee_Function.Label + System.Environment.NewLine;
                }
            }
            else
            {
                concernedScopes = "No scope defined for this employee yet.";
            }

            //Find out wich kind of email from the app Amaris.Mails should be called depending on the status of the material Request. Modifiers can be aplied.
            switch (materialRequest.OrderStatusId)
            {
                case OrderStatus.HelpDeskProductProposal:
                    EmailSender_OrderCreated(materialRequest, concernedEmployeeEmail, orderNumber, managerEmail, lomEmail, directorEmails, concernedEmployeeName, managerName, concernedScopes);
                    break;
                case OrderStatus.ProductDelivery:
                    EmailSender_OrderValidated(materialRequest, orderNumber);
                    break;
                case OrderStatus.ProductSetup:
                    EmailSender_OrderReadyForSetUp(materialRequest, orderNumber, concernedEmployeeName);
                    break;
                case OrderStatus.ProductDispatch:
                    break;
                case OrderStatus.Complete:
                    EmailSender_OrderReadyForDispatch(orderNumber, concernedEmployeeName, managerEmail, lomEmail);
                    break;
                case OrderStatus.Cancelled:
                    if (materialRequest.MaterialAction.Last().ActionId == (int)GActions.PurchasingDptCancelPrice ||
                        materialRequest.MaterialAction.Last().ActionId == (int)GActions.DirectorCancelPrice ||
                        materialRequest.MaterialAction.Last().ActionId == (int)GActions.ManagerCancelPrice)

                        EmailSender_OrderSuspended(materialRequest, concernedEmployeeEmail, orderedByEmployeeEmail, orderNumber, managerEmail);
                    else
                        EmailSender_OrderCancellation(materialRequest, orderedByEmployeeEmail, orderNumber, managerEmail, concernedEmployeeEmail);
                    break;
            }
        }

        private void EmailSender_OrderCreated(GATE_MaterialRequest materialRequest, string employeeEmail, int orderNumber, string managerEmail, string lomEmail, string directorEmails, string employeeName, string managerName, string employeeFunctions)
        {
            string amarisGateHomeUrl = _controller.Url.Action("Index", "OrderDetails", new { id = orderNumber }, _controller.Request.Url.Scheme);
            string materialNamesStr = "Empty";
            if (materialRequest.Package != null)
            {
                List<string> materialNamesList = materialRequest.Package.GenericMaterials.ToDictionary(mat => mat.GenericMaterialId, mat => mat.Label).Values.ToList();
                materialNamesStr = String.Join(",", materialNamesList);
            }

            Company workingForComp = materialRequest.ConcernedEmployee.WorkingForCompany;
            Boolean isConsultant = materialRequest.ConcernedEmployee.EmployeeType.EmployeeTypeId == 2;
            if (isConsultant)
            {
                _controllerDb.SendMail("NewConsultantArrivalNotification", new
                {
                    managerName = managerName,
                    employeeName = employeeName,
                    employeeFunctions = employeeFunctions,
                    employeeWorkingFor = (workingForComp != null ? workingForComp.CompanyCodeName : "Working for no company."),
                    managerEmail = managerEmail,
                    directorEmail = directorEmails,
                    LOMEmail = lomEmail,
                    orderNumber = orderNumber
                }, userId: _senderId, applicationName: _appName);
            }
            else
            {
                _controllerDb.SendMail("NewStaffRequestFromDNA", new
                {
                    managerEmail = managerEmail,
                    LOMEmail = lomEmail,
                    directorEmail = directorEmails,
                    employeeHostedByCompany = (materialRequest.ConcernedEmployee.HostedByCompany != null ? materialRequest.ConcernedEmployee.HostedByCompany.CompanyCodeName : "No company is hosting the concerned employee yet"),
                    packageName = (materialRequest.Package != null ? materialRequest.Package.PackageName : "No package assigned"),
                    employeeEmail = employeeEmail,
                    employeeName = employeeName,
                    employeeFunctions = employeeFunctions,
                    MaterialList = materialNamesStr,
                    linkToOrder = amarisGateHomeUrl,
                    orderNumber = orderNumber
                }, userId: _senderId, applicationName: _appName);
            }

        }
        
        private void EmailSender_OrderCancellation(GATE_MaterialRequest materialRequest, string requesterEmail, int orderNumber, string managerEmail, string employeeEmail)
        {
            var directorEmail = materialRequest.MaterialAction.Any(x => x.ActionId == (int)GActions.DirectorValidate
                                                                 || x.ActionId == (int)GActions.DirectorCancelPrice
                                                                 || x.ActionId == (int)GActions.DirectorCancelRelevancy) ?
                                                                                        materialRequest.MaterialAction.Last(x => x.ActionId == (int)GActions.DirectorValidate
                                                                                                                || x.ActionId == (int)GActions.DirectorCancelPrice
                                                                                                                || x.ActionId == (int)GActions.DirectorCancelRelevancy).Employee.Email
                                                                                        : "NoEmailDetected@Director.com";

            var purchasingOfficerEmail = materialRequest.MaterialAction.Any(x => x.ActionId == (int)GActions.SentForOrder
                                                                          || x.ActionId == (int)GActions.PurchasingDptCancelPrice
                                                                          || x.ActionId == (int)GActions.PurchasingDptCancelRelevancy) ?
                                                                                        materialRequest.MaterialAction.Last(x => x.ActionId == (int)GActions.SentForOrder
                                                                                                                || x.ActionId == (int)GActions.PurchasingDptCancelPrice
                                                                                                                || x.ActionId == (int)GActions.PurchasingDptCancelRelevancy).Employee.Email
                                                                                        : "NoEmailDetected@PurchasingOfficer.com";

            var orderRequestedDate = materialRequest.RequestDate.ToShortDateString();
            var cancellerName = materialRequest.MaterialAction.Last().Employee.Name;
            var cancellationDate = materialRequest.MaterialAction.Last().DetailDate;
            var cancellationComments = materialRequest.CancellationComment ?? "No comments";

            _controllerDb.SendMail("OrderCancellation", new
            {
                requesterEmail = requesterEmail,
                employeeEmail = employeeEmail,
                managerEmail = managerEmail,
                directorEmail = directorEmail,
                purchasingOfficerEmail = purchasingOfficerEmail,
                orderNumber = orderNumber.ToString(CultureInfo.InvariantCulture),
                orderRequestedDate = orderRequestedDate,
                cancellerName = cancellerName,
                cancellationDate = cancellationDate,
                cancellationComments = cancellationComments
            }, userId: _senderId, applicationName: _appName);
        }

        private void EmailSender_OrderSuspended(GATE_MaterialRequest materialRequest, string employeeEmail, string requesterEmail, int orderNumber, string managerEmail)
        {
            var helpDeskEmail = materialRequest.MaterialAction.Last(x => x.ActionId == (int)GActions.ExpertProposesProd).Employee.Email ?? "NoEmailDetected@Helpdesk.com";
            var directorEmail = materialRequest.MaterialAction.Any(x => x.ActionId == (int)GActions.DirectorValidate
                                                                 || x.ActionId == (int)GActions.DirectorCancelPrice
                                                                 || x.ActionId == (int)GActions.DirectorCancelRelevancy) ?
                                                                                        materialRequest.MaterialAction.Last(x => x.ActionId == (int)GActions.DirectorValidate
                                                                                                                || x.ActionId == (int)GActions.DirectorCancelPrice
                                                                                                                || x.ActionId == (int)GActions.DirectorCancelRelevancy).Employee.Email
                                                                                        : "NoEmailDetected@Director.com";
            var purchasingOfficerEmail = materialRequest.MaterialAction.Last(x => x.ActionId == (int)GActions.PurchasingDptCancelPrice).Employee.Email ?? "NoEmailDetected@PurchasingOfficer.com";
            var orderRequestedDate = materialRequest.RequestDate.ToShortDateString();
            var orderExpectedDate = (materialRequest.ExpectedDate.HasValue ? materialRequest.ExpectedDate.Value.Date.ToString() : "None");  
            var lastSuspenderName = materialRequest.MaterialAction.Last().Employee.Name ?? "";
            var lastSuspendingDate = materialRequest.MaterialAction.Last().Employee.Email ?? "NoEmailDetected@LastActionEmployee.com";

            var pendingOrderUrl = _controller.Url.Action("Index", "OrderDetails", new { id = orderNumber }, _controller.Request.Url.Scheme);

            _controllerDb.SendMail("OrderSuspended", new
            {
                orderNumber = orderNumber.ToString(CultureInfo.InvariantCulture),
                helpDeskOfficerEmail = helpDeskEmail,
                employeeEmail = employeeEmail,
                requesterEmail = requesterEmail,
                managerEmail = managerEmail,
                directorEmail = directorEmail,
                purchasingOfficerEmail = purchasingOfficerEmail,
                orderRequestedDate = orderRequestedDate,
                orderExpectedDate = orderExpectedDate,
                lastSuspenderName = lastSuspenderName,
                lastSuspendingDate = lastSuspendingDate,
                pendingOrderURL = pendingOrderUrl
            }, userId: _senderId, applicationName: _appName);
        }


        private void EmailSender_OrderValidated(GATE_MaterialRequest materialRequest, int orderNumber)
        {
            string linkToOrder = _controller.Url.Action("Index", "OrderDetails", new { id = orderNumber }, _controller.Request.Url.Scheme);

            _controllerDb.SendMail("purchasingDptNotification", new
            {
                orderNumber = orderNumber,
                LinkToOrder = linkToOrder
            }, userId: _senderId, applicationName: _appName);
        }

        private void EmailSender_OrderReadyForDispatch(int orderNumber, string employeeName, string managerEmail, string lomEmail)
        {

            _controllerDb.SendMail("OrderReadyToDispatch", new
            {
                orderNumber = orderNumber,
                employeeName = employeeName,
                managerEmail = managerEmail,
                lomEmail = lomEmail
            }, userId: _senderId, applicationName: _appName);
        }

        private void EmailSender_OrderReadyForSetUp(GATE_MaterialRequest materialRequest, int orderNumber, string concernedEmployeeName)
        {
            IEnumerable<GATE_ComponentRequest> components = materialRequest.ComponentRequests.Where(y => y.IsSetup == false);
            List<string> productSetupRequiredList = new List<string>();

            foreach (GATE_ComponentRequest productRequired in components)
            {
                productSetupRequiredList.Add(productRequired.GenericMaterial.Label);
            }

            var pendingSetupUrl = _controller.Url.Action("Index", "OrderDetails", new { id = orderNumber }, _controller.Request.Url.Scheme);

            _controllerDb.SendMail("SetupNeeded", new
            {
                employeeName = concernedEmployeeName,
                orderNumber = orderNumber,
                productSetupRequiredList = string.Join("<br/> - ", productSetupRequiredList),
                linkToOrder =  "<a href=\"" + pendingSetupUrl + "\"> here!</a>"
            }, userId: _senderId, applicationName: _appName);
        }


    }
}