using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Amaris.Mails;
using Amaris.Security;
using Amaris.Statistics;
using AmarisGate.Dal;
using AmarisGate.Model;
using AmarisGate.Model.Pending;
using Developpez.Dotnet;
using MailService.Models;
using AmarisGate.Helpers;

namespace AmarisGate.Controllers
{
    public class HomeController : BootstrapBaseController
    {

        #region Views

        public const int EntriesPerPage = 20;

        [HttpGet]
        [UsageCounter]
        public ActionResult Index(int? page)
        {
            return RedirectToAction("Pending", new { page = page });
        }

        public void FixComponentSuggestions()
        {
            List<GATE_ComponentRequest> comps = DB.GATE_ComponentRequest.Where(com => com.CurrentSuggestion == null).ToList();
            foreach (GATE_ComponentRequest theComponent in comps)
            {
                GATE_MaterialSuggestion newSuggestion = new GATE_MaterialSuggestion
                {
                    InStock = false,
                    IsNotStandardProduct = false,
                    Label = string.Empty
                };

                DB.GATE_MaterialSuggestion.Add(newSuggestion);
                theComponent.CurrentSuggestion = newSuggestion;
            }
            DB.SaveChanges();
        }



        [HttpGet]
        [UsageCounter]

        public ActionResult Pending()
        {
            ViewBag.isSearchAboutEmployee = false;
            ViewBag.LoggedUser = LoggedUser;
            var requestsModel = DB.GATE_MaterialRequest
                .WithTreeSecurity(LoggedUser.EmployeeId, false)
                .Where(
                    req =>
                        req.OrderStatusId != OrderStatus.Cancelled
                        && req.OrderStatusId != OrderStatus.Complete)
                        .OrderBy(req => req.ConcernedEmployee.EntryDate)
                .AsEnumerable()
                .Select(p => new RequestViewModel(p))
                //.Select(RequestViewModel.ContructorForEF);
                //.Select(p => new RequestViewModel.ContructorForEF);
                .OrderBy(req => req.TodoPriority)
                .ThenBy(req => req.ConcernedEmployee.EntryDate);

            SearchViewModel searchModel = new SearchViewModel(DB.EmployeeStatus, DB.EmployeeTypes, DB.GATE_OrderStatus);
            TableViewModel tableModel = new TableViewModel(DB.Packages, requestsModel);
            //To use 76 for Page, set PageNum = 1 in line 60
            PendingViewModel pendingModel = new PendingViewModel(searchModel, tableModel);

            //Reset session variable
            Session["SelectedEmployeeId"] = null;

            return View(pendingModel);
        }

        [HttpGet]
        [UsageCounter]
        public ActionResult PendingToDo()
        {
            IEnumerable<RequestViewModel> requestsModel = DB.GATE_MaterialRequest
                .WithTreeSecurity(LoggedUser.EmployeeId, false)
                .Where(
                    req =>
                        req.OrderStatusId != OrderStatus.Cancelled
                        && req.OrderStatusId != OrderStatus.Complete)
                .OrderBy(req => req.ConcernedEmployee.EntryDate)
                .Select(RequestViewModel.ContructorForEF)
                .OrderBy(req => req.TodoPriority).ThenBy(req => req.ConcernedEmployee.EntryDate);

            TodoTableViewModel tableModel = new TodoTableViewModel(DB.Packages, requestsModel);
            return View(tableModel);
        }

        [HttpGet]
        [UsageCounter]
        public ActionResult SearchTable(UserQuery query, int pageNum = 1)
        {
            var requestsModel = DB.GATE_MaterialRequest
                .Where(req => !query.HasEmployees || query.EmployeeIds.Contains(req.ConcernedEmployeeId))
                .Where(req => !query.HasOffices || !req.AmarisOfficeId.HasValue || query.OfficeIds.Contains(req.AmarisOfficeId.Value))
                .Where(req => !query.HasEmployeeStatuses || !req.ConcernedEmployee.EmployeeStatusId.HasValue || query.EmployeeStatusesIds.Contains(req.ConcernedEmployee.EmployeeStatusId.Value))
                .Where(req => !query.HasType || req.ConcernedEmployee.EmployeeTypeId == query.TypeId)
                .Where(req => !query.HasFunctions || req.ConcernedEmployee.Employee_Scope.Any(sco => query.FunctionIds.Contains(sco.FunctionId)))
                .Where(req => req.OrderStatusId != OrderStatus.Cancelled)
                .Where(req => !query.HasOrderStatus || req.OrderStatusId == (OrderStatus?)query.OrderStatusId)
                .WithTreeSecurity(LoggedUser.EmployeeId, false);

            if (!query.HasEmployees)
            {
                requestsModel = requestsModel.Where(req => req.OrderStatusId != OrderStatus.Complete);
            }

            IEnumerable<RequestViewModel> model = requestsModel.Include(x => x.ConcernedEmployee).Select(RequestViewModel.ContructorForEF);
            if (query.OrderbyEntryDate)
            {
                model = query.OrderbyDesc ? model.OrderByDescending(req => req.ConcernedEmployee.EntryDate)
                        : model.OrderBy(req => req.ConcernedEmployee.EntryDate);
            }
            else if (query.OrderbyDeliveryDate)
            {
                model = query.OrderbyDesc ? model.OrderByDescending(req => req.ExpectedDate)
                        : model.OrderBy(req => req.ExpectedDate);
            }
            else
            {
                model = model.OrderBy(req => req.OrderId);
            }

            //var tableModel = new TableViewModel(DB.Packages, model.ToPagedList(pageNum, EntriesPerPage), query);
            var tableModel = new TableViewModel(DB.Packages, model, query);

            return PartialView("_PendingTable", tableModel);
        }

        [HttpGet]
        [UsageCounter]
        public ActionResult RequestDetails(int id)
        {
            GATE_MaterialRequest order = DB.GATE_MaterialRequest
                .Include(x => x.ConcernedEmployee)
                .Include(x => x.OrderedByEmployee)
                .Include(x => x.Package)
                .Include(x => x.Package.GenericMaterials)
                .Include(x => x.ComponentRequests)
                .Where(x => x.MaterialRequestId == id)
                .Select(x => x).FirstOrDefault();

            string title = "Details of order #" + order.MaterialRequestId;
            string conceredEmployeeName = order.ConcernedEmployee.Name;
            string orderedBy = order.IsAutomated == false ? "DNA Request." : "Requested By " + order.OrderedByEmployee.Name + ".";

            IEnumerable<string> materialLables;
            if (order.PackageId != null)
            {
                materialLables = order.Package.GenericMaterials.ToDictionary(mat => mat.GenericMaterialId, mat => mat.Label).Values;
            }
            else
            {
                materialLables = order.ComponentRequests.ToDictionary(com => com.MaterialId, com => com.GenericMaterial.Label).Values;
            }
            string components = string.Join(" | ", materialLables);

            ModalDetails model = new ModalDetails(title, "requestModal", orderedBy, conceredEmployeeName, order.IsAutomated, components, order.MaterialAction);
            return PartialView("_PopupDetails", model);
        }

        [UsageCounter]
        public ActionResult CancelDetails(int id)
        {
            RequestViewModel model = DB.GATE_MaterialRequest
                .Where(x => x.MaterialRequestId == id)
                .Select(RequestViewModel.ContructorForEF)
                .FirstOrDefault();
            return PartialView("_CancelDetails", model);
        }

        // This action for : when we Set "Delivery Address" from "No Office" -> some office , page will reload , so we can edit EDD also.
        [HttpPost]
        public ActionResult ReloadPendingRow()
        {
            IEnumerable<RequestViewModel> requestsModel = DB.GATE_MaterialRequest
                .WithTreeSecurity(LoggedUser.EmployeeId, false)
                .Where(req => req.OrderStatusId != OrderStatus.Cancelled && req.OrderStatusId != OrderStatus.Complete)
                .OrderBy(req => req.ConcernedEmployee.EntryDate)
                .Select(RequestViewModel.ContructorForEF)
                .OrderBy(req => req.TodoPriority).ThenBy(req => req.ConcernedEmployee.EntryDate);

            return PartialView("_PendingRowModel", requestsModel.ToList());
        }


        #endregion

        #region Material Process methods

        [HttpPost]
        [UsageCounter]
        public ActionResult ResetOrderStatus(int id)
        {
            try
            {
                GATE_MaterialRequest oldRequest = DB.GATE_MaterialRequest
                    .Include(x => x.MaterialAction)
                    .Include(x => x.Message)
                    .Include(x => x.Package)
                    .Include(x => x.Package.GenericMaterials)
                    .First(x => x.MaterialRequestId == id);
                oldRequest.OrderStatusId = OrderStatus.Cancelled;
                oldRequest.CancellationComment = "Order reset";

                GATE_MaterialRequest newRequest = new GATE_MaterialRequest
                {
                    OrderStatusId = OrderStatus.HelpDeskProductProposal,
                    ConcernedEmployeeId = oldRequest.ConcernedEmployeeId,
                    OrderedByEmployeeId = oldRequest.OrderedByEmployeeId,
                    PackageId = oldRequest.PackageId,
                    RequestDate = oldRequest.RequestDate,
                    ExpectedDate = oldRequest.ExpectedDate,
                    Comment = oldRequest.Comment,
                    AddressId = oldRequest.AddressId,
                    AmarisOfficeId = oldRequest.AmarisOfficeId,
                    CurrencyId = oldRequest.CurrencyId,
                    OrderTypeId = oldRequest.OrderTypeId,
                    TotalCost = oldRequest.TotalCost,
                    IsAutomated = oldRequest.IsAutomated,
                    MaterialAction = oldRequest.MaterialAction.ToList(),
                    Message = oldRequest.Message.ToList()
                };

                //cloning the old list using ToList()


                //Request the individual components (aparently this is necessary, although I am not sure why ...)
                Package thePackage = oldRequest.Package;
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

                    DB.GATE_ComponentRequest.Add(component);
                }

                //Adding new Material Actions and Messages
                foreach (MaterialAction action in newRequest.MaterialAction)
                {
                    DB.MaterialActions.Add(action);
                }

                foreach (Message message in newRequest.Message)
                {
                    DB.Messages.Add(message);
                }

                DB.GATE_MaterialRequest.Add(newRequest);
                DB.SaveChanges();
                DB.GATE_MaterialRequest.UpdatePermissions(newRequest.MaterialRequestId);


                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult ConfirmDelivery(int id)
        {
            try
            {
                GATE_MaterialRequest materialRequest = DB.GATE_MaterialRequest.Find(id);
                MaterialAction newAction;

                //if none of the materials requires setup, we move to the next stage
                if (materialRequest.PackageId != null && materialRequest.Package.GenericMaterials.All(mat => !mat.RequiresSetup))
                {
                    materialRequest.OrderStatusId = OrderStatus.ProductDispatch;
                    newAction = new MaterialAction
                    {
                        ActionId = (int)GActions.SetUpComplete,
                        EmployeeId = LoggedUser.EmployeeId,
                        DetailDate = DateTime.Now,
                        MaterialRequestId = materialRequest.MaterialRequestId
                    };

                }
                else
                {
                    materialRequest.OrderStatusId = OrderStatus.ProductSetup;
                    newAction = new MaterialAction
                    {
                        ActionId = (int)GActions.Delivered,
                        EmployeeId = LoggedUser.EmployeeId,
                        DetailDate = DateTime.Now,
                        MaterialRequestId = materialRequest.MaterialRequestId
                    };

                }

                DB.MaterialActions.Add(newAction);
                DB.SaveChanges();
                MailMan.SendAlertMails(materialRequest);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult ConfirmProposal(int id)
        {
            try
            {
                var materialReq = DB.GATE_MaterialRequest.Find(id);

                if (materialReq.TotalCost != null)
                {
                    //we only take care of autmated requests for now. If you want to take care of Manual, add an if statement:
                    /*
                     * if(materialReq.IsAutomated){
                     *     //copy paste the following code here
                     * }else{
                     *     //code for manual requets here
                     * }
                     */
                    materialReq.OrderStatusId = OrderStatus.ProductDelivery;
                    MailMan.SendAlertMails(materialReq);

                    var newAction = new MaterialAction
                    {
                        ActionId = (int)GActions.MaterialSetUp,
                        EmployeeId = LoggedUser.EmployeeId,
                        DetailDate = DateTime.Now,
                        MaterialRequestId = materialReq.MaterialRequestId
                    };
                    DB.MaterialActions.Add(newAction);
                    DB.SaveChanges();
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Before Validation you must propose all products and set total price");
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult ConfirmSetUp(int id)
        {
            try
            {
                var materialReq = DB.GATE_MaterialRequest.Find(id);

                if (materialReq.ComponentRequests.All(x => x.IsSetup))
                {
                    materialReq.OrderStatusId = OrderStatus.ProductDispatch;

                    var newAction = new MaterialAction
                    {
                        ActionId = (int)GActions.SetUpComplete,
                        EmployeeId = LoggedUser.EmployeeId,
                        DetailDate = DateTime.Now,
                        MaterialRequestId = materialReq.MaterialRequestId
                    };
                    DB.MaterialActions.Add(newAction);
                    DB.SaveChanges();

                    MailMan.SendAlertMails(materialReq);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Before Validation you must mark all products as set up");
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
            //return new RedirectResult(Url.Action("Pending") + "#tr" + materialReq.MaterialRequestId);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult ConfirmDispatch(int id)
        {
            try
            {
                GATE_MaterialRequest materialReq = DB.GATE_MaterialRequest.Find(id);

                if (materialReq.ComponentRequests.All(x => x.IsDispatched))
                {
                    materialReq.OrderStatusId = OrderStatus.Complete;

                    var newAction = new MaterialAction
                    {
                        ActionId = (int)GActions.Dispatched,
                        EmployeeId = LoggedUser.EmployeeId,
                        DetailDate = DateTime.Now,
                        MaterialRequestId = materialReq.MaterialRequestId
                    };
                    DB.MaterialActions.Add(newAction);
                    DB.SaveChanges();

                    MailMan.SendAlertMails(materialReq);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Before Validation you must mark all products as dispatched");
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
            //return new RedirectResult(Url.Action("Pending") + "#tr" + materialReq.MaterialRequestId);
        }

        #endregion

        #region Ajax and XEditable Functions

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateComponentRequestInStock(int pk, string name, string value)
        {
            GATE_MaterialSuggestion currentSuggestion = DB.GATE_ComponentRequest.Find(pk).CurrentSuggestion;
            Boolean parsedValue;
            Boolean.TryParse(value, out parsedValue);
            currentSuggestion.InStock = parsedValue;
            DB.SaveChanges();

            return XEditableUpdate(DB.GATE_ComponentRequest, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateMaterialSuggestionLabel(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialSuggestion, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateMaterialSuggestionDeliveryDate(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialSuggestion, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateMaterialSuggestionPrice(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialSuggestion, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateMaterialSuggestionCurrency(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialSuggestion, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult CancelOrder(int id)
        {
            try
            {
                GATE_MaterialRequest matReq = DB.GATE_MaterialRequest.Find(id);

                matReq.CancellationComment = "Canceled at " + DateTime.Now;

                MaterialAction deleteAction = new MaterialAction
                {
                    ActionId = (int)GActions.Cancelled,
                    EmployeeId = LoggedUser.EmployeeId,
                    DetailDate = DateTime.Now,
                    Comment = "Order deleted by " + LoggedUser.Name,
                    MaterialRequestId = id
                };

                matReq.OrderStatusId = OrderStatus.Cancelled;
                matReq.MaterialAction.Add(deleteAction);

                DB.MaterialActions.Add(deleteAction);
                DB.SaveChanges();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateOrderPackage(int reqId, string pkgName)
        {
            try
            {
                GATE_MaterialRequest matReq = DB.GATE_MaterialRequest.Find(reqId);
                Package pkg = DB.Packages.FirstOrDefault(aPkg => aPkg.PackageName.Equals(pkgName));

                DB.GATE_ComponentRequest.RemoveRange(matReq.ComponentRequests);
                matReq.PackageId = pkg.PackageId;
                foreach (var material in pkg.GenericMaterials)
                {
                    GATE_ComponentRequest component = new GATE_ComponentRequest
                    {
                        MaterialId = material.GenericMaterialId,
                        IsSetup = !material.RequiresSetup,
                        IsDispatched = false,
                        MaterialRequestId = matReq.MaterialRequestId
                    };
                    DB.GATE_ComponentRequest.Add(component);
                }

                DB.SaveChanges();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [UsageCounter]
        public ActionResult UpdateExpectedDate(int pk, string name, string value)
        {
            GATE_MaterialRequest request = DB.GATE_MaterialRequest.Find(pk);
            Employee concernedEmployee = request.ConcernedEmployee;
            Employee manager = request.ConcernedEmployee.Manager;
            Employee lom = LoggedUser;

            //Update date of all components
            foreach (GATE_ComponentRequest component in request.ComponentRequests)
            {
                component.CurrentSuggestion.DeliveryDate = Convert.ToDateTime(value);
            }

            //Create appointment
            DateTime deliveryDay = Convert.ToDateTime(value).Date;

            string deliveryDate = (!request.ExpectedDate.Equals(DateTime.MinValue) && request.ExpectedDate != null ? request.ExpectedDate.Value.Date.ToShortDateString() : "No date defined");
            string officeName = (request.Office != null ? request.Office.Name : (request.Address != null ? request.Address.Complement : "No destination defined"));
            string linkToOrder = Url.Action("Index", "OrderDetails", new { id = request.MaterialRequestId }, Request.Url.Scheme);


            MessageTemplateOptions template = new MessageTemplateOptions("NewDeliveryDateExpected",
                new
                {
                    managerName = manager.Name,
                    managerEmail = manager.Email,
                    lomEmail = lom.Email,
                    concernedEmployeeName = concernedEmployee.Name,
                    officeName,
                    deliveryDate,
                    linkToOrder,
                    orderNumber = request.MaterialRequestId
                });
            template.ApplicationName = "AmarisGate";
            template.UserId = LoggedUser.EmployeeId;

            AppointmentModel appointment = new AppointmentModel(deliveryDay, deliveryDay.AddHours(24))
            {
                IsAllDayEvent = true,
                Categories = new List<string> { "AmarisGate", "Project" },
                Location = officeName
            };

            request.AddAppointment(template, appointment);

            if (name == null)
                name = "ExpectedDate";

            return XEditableUpdate(DB.GATE_MaterialRequest, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateOwnerEmployee(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialRequest, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult XEditOrderPackage(int pk, string name, string value)
        {
            GATE_MaterialRequest request = DB.GATE_MaterialRequest.Find(pk);
            Package thePackage = DB.Packages.Find(Int32.Parse(value));

            List<GATE_ComponentRequest> components = new List<GATE_ComponentRequest>();
            foreach (var material in thePackage.GenericMaterials)
            {
                GATE_ComponentRequest component = new GATE_ComponentRequest
                {
                    MaterialId = material.GenericMaterialId,
                    IsSetup = material.RequiresSetup,
                    MaterialRequestId = request.MaterialRequestId,
                    StatusId = OrderStatus.HelpDeskProductProposal
                };

                components.Add(component);
                DB.GATE_ComponentRequest.Add(component);

            }

            DB.GATE_ComponentRequest.RemoveRange(request.ComponentRequests);
            request.ComponentRequests = components;

            DB.SaveChanges();

            foreach (GATE_ComponentRequest component in request.ComponentRequests)
            {
                GATE_MaterialSuggestion newSuggestion = new GATE_MaterialSuggestion
                {
                    InStock = false,
                    IsNotStandardProduct = false,
                    Label = string.Empty
                };

                DB.GATE_MaterialSuggestion.Add(newSuggestion);
                component.CurrentSuggestion = newSuggestion;
            }
            DB.SaveChanges();

            return XEditableUpdate(DB.GATE_MaterialRequest, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateDeliveryAddress(int pk, string name, string value)
        {
            GATE_MaterialRequest request = DB.GATE_MaterialRequest.Where(x => x.MaterialRequestId == pk).Select(x => x).FirstOrDefault();
            if (request.Address != null)
            {
                request.Address.Complement = string.Empty;
            }
            else
            {
                Address newAddressObj = new Address
                {
                    Complement = string.Empty,
                    CreatedDate = DateTime.Now
                };
                DB.Addresses.Add(newAddressObj);
                DB.SaveChanges();
                request.Address = newAddressObj;
            }


            return XEditableUpdate(DB.GATE_MaterialRequest, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateCustomDeliveryAddress(int reqId, string newAddress)
        {
            try
            {
                GATE_MaterialRequest matReq = DB.GATE_MaterialRequest.Find(reqId);

                if (matReq.Address == null)
                {
                    Address newAddressObj = new Address
                    {
                        Complement = newAddress,
                        CreatedDate = DateTime.Now
                    };
                    DB.Addresses.Add(newAddressObj);
                    DB.SaveChanges();
                    matReq.Address = newAddressObj;
                }
                else
                {
                    matReq.Address.Complement = newAddress;
                }

                DB.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public ActionResult AmarisOffices(string query = "", int? id = null)
        {
            if (id != null)
            {
                Office office = DB.Offices.Find(id);
                return Json(new { id, text = office.Name }, JsonRequestBehavior.AllowGet);
            }

            var officesList = DB.Offices.Where(x => x.Name.ToLower().Contains(query.ToLower())).Select(anOffice => new { id = anOffice.OfficeId, text = anOffice.Name }).ToList();
            return Json(officesList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Packages(string query, int? id)
        {
            query = query.IsNullOrEmpty() ? null : query.ToLower();
            var packages = DB.Packages
                .Where(x => query == null || x.PackageName.ToLower().Contains(query))
                .Where(x => id == null || x.PackageId == id)
                .Select(x => new
        {
            id = x.PackageId,
            text = x.PackageName
        });
            return id != null ? Json(packages.FirstOrDefault(), JsonRequestBehavior.AllowGet) : Json(packages, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Employees(string query)
        {
            if (query.Length < 3)
                return Json(string.Empty, JsonRequestBehavior.AllowGet);
            var selectList =
                DB.Employees.Where(em => em.EmployeeStatusId != 4 && (query.IsNullOrEmpty()
                    || em.Trigram.ToLower().Contains(query.ToLower())
                    || em.Name.ToLower().Contains(query.ToLower()))).Select(e => new
                {
                    id = e.EmployeeId,
                    text = e.Trigram + " | " + e.Name
                });
            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult EmployeesMultiple(string query = "", string id = null)
       {
           if (query.Length < 3)
               return Json(string.Empty, JsonRequestBehavior.AllowGet);

            if (id != null)
            {
                int[] ids = Array.ConvertAll<string, int>(id.Split(','), int.Parse);
                var res = DB.Employees
                .Where(emp => ids.Contains(emp.EmployeeId))
                .Select(emp => new
                    {
                        id = emp.EmployeeId,
                        text = emp.Lastname + " " + emp.Firstname
                    }
                );

                return Json(res, JsonRequestBehavior.AllowGet);
            }

            var selectList = DB.Employees
                .Where(emp =>
                    (
                        query.Trim().Equals(string.Empty) ||
                        (emp.Trigram != null && emp.Trigram.Trim().ToLower().Contains(query.Trim().ToLower())) ||
                        (emp.Lastname + " " + emp.Firstname).ToLower().Contains(query.Trim().ToLower()) ||
                        (emp.Firstname + " " + emp.Lastname).ToLower().Contains(query.Trim().ToLower()))
                    )
                .OrderBy(emp => emp.Lastname)
                .Select(emp => new
                {
                    id = emp.EmployeeId,
                    text = emp.Lastname + " " + emp.Firstname
                }).ToList();

            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult EmployeesSingle(string query = "", int? id = null)
        {
            if (id != null)
            {
                var res = DB.Employees.Find(id);

                return Json(new { id = res.EmployeeId, text = res.FullName }, JsonRequestBehavior.AllowGet);
            }

            var selectList = DB.Employees
                .ToList()
                .Where(emp =>
                    (
                        query.Trim().Equals(string.Empty) ||
                        (emp.Trigram != null && emp.Trigram.Trim().ToLower().Contains(query.Trim().ToLower())) ||
                        (emp.Lastname + " " + emp.Firstname).ToLower().Contains(query.Trim().ToLower()) ||
                        (emp.Firstname + " " + emp.Lastname).ToLower().Contains(query.Trim().ToLower()))
                    )
                .OrderBy(emp => emp.Lastname)
                .Select(emp => new
                {
                    id = emp.EmployeeId,
                    text = emp.Lastname + " " + emp.Firstname
                }).ToList();

            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult EmployeesOrOfficesMultiple(string query = "", string id = null)
        {
            var selectList = DB.Employees
                .Where(emp =>
                    (
                        !query.Trim().Equals(string.Empty) &&
                        ((emp.Trigram != null && emp.Trigram.Trim().ToLower().Contains(query.Trim().ToLower())) ||
                        (emp.Lastname + " " + emp.Firstname).ToLower().Contains(query.Trim().ToLower()) ||
                        (emp.Firstname + " " + emp.Lastname).ToLower().Contains(query.Trim().ToLower())))
                    )
                .OrderBy(emp => emp.Lastname)
                .Select(emp => new
                {
                    type = "employee",
                    id = emp.EmployeeId,
                    text = "Employee : " + emp.Lastname + " " + emp.Firstname
                }).ToList();
            selectList.AddRange(DB.Offices.Where(x => x.Name.ToLower().Contains(query.ToLower())).Select(anOffice => new { type = "office", id = anOffice.OfficeId, text = "Office : " + anOffice.Name }).ToList());
            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FunctionsMultiple(string query = "", string id = null)
        {

            if (id != null)
            {
                int[] ids = Array.ConvertAll<string, int>(id.Split(','), int.Parse);

                var res =
                DB.Employee_Function.Where(fun => ids.Contains(fun.FunctionId)).Select(fun => new
                {
                    id = fun.FunctionId,
                    text = fun.Label
                });

                return Json(res, JsonRequestBehavior.AllowGet);
            }

            var selectList =
            DB.Employee_Function.Where(fun =>
               fun.Label.Trim().ToLower().Contains(query.Trim().ToLower())).OrderBy(res => res.Label).Select(fun => new
               {
                   id = fun.FunctionId,
                   text = fun.Label
               });

            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MaterialsMultiple(string query = "", string id = null)
        {
            if (id != null)
            {
                int[] ids = Array.ConvertAll<string, int>(id.Split(','), int.Parse);
                var res =
                DB.GenericMaterials.Where(mat => ids.Contains(mat.GenericMaterialId)).Select(mat => new
                   {
                       id = mat.GenericMaterialId,
                       text = mat.Label
                   });
                return Json(res, JsonRequestBehavior.AllowGet);
            }

            var selectList =
                DB.GenericMaterials.Where(mat =>
                   mat.Label.ToLower().Contains(query.ToLower())).OrderBy(x => x.Label).Select(mat => new
                   {
                       id = mat.GenericMaterialId,
                       text = mat.Label
                   });
            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CompaniesMultiple(string query = "", string id = null)
        {
            if (id != null)
            {
                int[] ids = Array.ConvertAll<string, int>(id.Split(','), int.Parse);
                ViewBag.LoggedUser = LoggedUser;
                var res = DB.Companies
                    .Where(comp => ids.Contains(comp.ID))
                    .Select(comp => new
                    {
                        id = comp.ID,
                        text = comp.CompanyCodeName
                    }
                );

                return Json(res, JsonRequestBehavior.AllowGet);
            }

            var selectList = DB.Companies
                .Where(com => com.Active && (query == "" || com.CompanyCodeName != null && com.CompanyCodeName.ToLower().Contains(query.ToLower())))
                .OrderBy(com => com.CompanyCodeName)
                .Select(obj => new
                {
                    id = obj.ID,
                    text = obj.CompanyCodeName
                });

            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [UsageCounter]
        public ActionResult UpdateCurrency(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialRequest, pk, name, value);
        }

        [UsageCounter]
        public ActionResult UpdateTotalCost(int pk, string name, string value)
        {
            name = "TotalCost";
            var valueReplace = value.Replace(",", ".");
            var result = XEditableUpdate(DB.GATE_MaterialRequest, pk, name, valueReplace);

            return result;
        }

        [UsageCounter]
        public ActionResult UpdateSuggestion(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialSuggestion, pk, name, value);
        }

        public JsonResult RequestCurrencies(string query = "")
        {
            var currencies = DB.Currencies
                .Where(x => query == String.Empty || x.Label.ToLower().Contains(query.ToLower()) || x.ID.ToLower().Contains(query.ToLower()))
                .OrderBy(x => x.Label)
                .Select(item => new
                {
                    id = item.ID,
                    text = item.ID + " - " + item.Label
                }).ToList();
            return Json(currencies, JsonRequestBehavior.AllowGet);
        }

        [UsageCounter]
        public ActionResult UpdateComponentDispatch(int pk, string name, string value)
        {
            if (name == null)
                name = "IsDispatched";
            return XEditableUpdate(DB.GATE_ComponentRequest, pk, name, value);
        }

        [UsageCounter]
        public ActionResult UpdateComponentSetup(int pk, string name, string value)
        {
            if (name == null)
                name = "IsSetup";

            return XEditableUpdate(DB.GATE_ComponentRequest, pk, name, value);
        }

        [UsageCounter]
        public ActionResult UpdateComment(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialRequest, pk, name, value);
        }

        public ActionResult GetTotalCostPartial(OrderDetailsModel model)
        {
            return PartialView("~/Views/OrderDetails/_TotalCost.cshtml", model);
        }

        #endregion

        #region CRUD Suggestion methods



        [HttpPost]
        [ValidateAntiForgeryToken]
        [UsageCounter]
        public ActionResult CreateSuggestion(GATE_ComponentRequest suggestion, HttpPostedFileWrapper pictureUpload)
        {
            //TODO reimplement with the new Stock and password tables #stock
            if (!suggestion.InStock)
            {
                if (suggestion.CurrentSuggestion.Price == null)
                {
                    ModelState.AddModelError("MaterialSuggestion.Price", "Price is required");
                }

                if (suggestion.CurrentSuggestion.Label == null)
                {
                    ModelState.AddModelError("MaterialSuggestion.Label", "Label is required");
                }
            }
            else
            {
                suggestion.CurrentSuggestion.Label = "In Stock";
                suggestion.CurrentSuggestion.Price = 0;
            }


            if (pictureUpload != null && pictureUpload.ContentType.Substring(0, 5) != "image")
            {
                ModelState.AddModelError("pictureUpload", "The file you tried to insert is not an image file, you should use .jpg, .png or .gif");
            }

            var componentRequest = DB.GATE_ComponentRequest.Find(suggestion.ComponentId);

            GATE_MaterialSuggestion matSuggestion = suggestion.CurrentSuggestion;
            componentRequest.CurrentSuggestion = matSuggestion;

            if (pictureUpload != null && pictureUpload.ContentType.Substring(0, 5) == "image")
            {
                var b = new BinaryReader(pictureUpload.InputStream);
                byte[] binData = b.ReadBytes(pictureUpload.ContentLength);
                var pictureFile = new GATE_SuggestionPicture
                {
                    PictureData = binData,
                };
                componentRequest.CurrentSuggestion.GATE_SuggestionPicture = pictureFile;
            }
            else
            {
                componentRequest.CurrentSuggestion.PictureId = null;
            }

            DB.GATE_MaterialSuggestion.Add(matSuggestion);
            DB.SaveChanges();

            return new RedirectResult(Url.Action("Pending") + "#tr" + componentRequest.MaterialRequestId);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [UsageCounter]
        public ActionResult EditSuggestion(GATE_ComponentRequest suggestion, HttpPostedFileWrapper pictureUpload = null)
        {
            if (!suggestion.InStock)
            {
                if (suggestion.CurrentSuggestion.Price == null)
                {
                    ModelState.AddModelError("MaterialSuggestion.Price", "Price is required");
                }

                if (suggestion.CurrentSuggestion.Label == null)
                {
                    ModelState.AddModelError("MaterialSuggestion.Label", "Label is required");
                }
            }
            else
            {
                suggestion.CurrentSuggestion.Label = "In Stock";
                suggestion.CurrentSuggestion.Price = 0;
            }


            if (pictureUpload != null && pictureUpload.ContentType.Substring(0, 5) != "image")
            {
                ModelState.AddModelError("pictureUpload", "The file you tried to insert is not an image file, you should use .jpg, .png or .gif");
            }

            var componentRequest = DB.GATE_ComponentRequest.Include(x => x.CurrentSuggestion)
                                                            .Include(x => x.PreviousSuggestion)
                                                            .Where(x => x.ComponentId == suggestion.ComponentId)
                                                            .Select(x => x).FirstOrDefault();

            if (componentRequest.PreviousMaterialSuggestedId == null)
            {
                var matSuggestion = new GATE_MaterialSuggestion
                {
                    Label = componentRequest.CurrentSuggestion.Label,
                    Price = componentRequest.CurrentSuggestion.Price,
                    DeliveryDate = componentRequest.CurrentSuggestion.DeliveryDate,
                    CurrencyId = componentRequest.CurrentSuggestion.CurrencyId,
                    IsNotStandardProduct = componentRequest.CurrentSuggestion.IsNotStandardProduct,
                    PictureId = componentRequest.CurrentSuggestion.PictureId,
                    InStock = componentRequest.CurrentSuggestion.InStock
                };
                componentRequest.PreviousSuggestion = matSuggestion;
                DB.GATE_MaterialSuggestion.Add(componentRequest.PreviousSuggestion);
                DB.SaveChanges();
            }
            else
            {
                componentRequest.PreviousSuggestion.Label = componentRequest.CurrentSuggestion.Label;
                componentRequest.PreviousSuggestion.Price = componentRequest.CurrentSuggestion.Price;
                componentRequest.PreviousSuggestion.DeliveryDate = componentRequest.CurrentSuggestion.DeliveryDate;
                componentRequest.PreviousSuggestion.CurrencyId = componentRequest.CurrentSuggestion.CurrencyId;
                componentRequest.PreviousSuggestion.IsNotStandardProduct = componentRequest.CurrentSuggestion.IsNotStandardProduct;
                componentRequest.PreviousSuggestion.PictureId = componentRequest.CurrentSuggestion.PictureId;
                componentRequest.PreviousSuggestion.InStock = componentRequest.CurrentSuggestion.InStock;
                DB.Entry(componentRequest.PreviousSuggestion).State = EntityState.Modified;
                DB.SaveChanges();
            }
            componentRequest.CurrentSuggestion.Label = suggestion.CurrentSuggestion.Label;
            componentRequest.CurrentSuggestion.Price = suggestion.CurrentSuggestion.Price;
            componentRequest.CurrentSuggestion.DeliveryDate = suggestion.CurrentSuggestion.DeliveryDate;
            componentRequest.CurrentSuggestion.CurrencyId = suggestion.CurrentSuggestion.CurrencyId;
            componentRequest.CurrentSuggestion.IsNotStandardProduct = suggestion.CurrentSuggestion.IsNotStandardProduct;
            componentRequest.CurrentSuggestion.InStock = suggestion.InStock;

            if (pictureUpload != null && pictureUpload.ContentType.Substring(0, 5) == "image")
            {
                var b = new BinaryReader(pictureUpload.InputStream);
                byte[] binData = b.ReadBytes(pictureUpload.ContentLength);
                var pictureFile = new GATE_SuggestionPicture { PictureData = binData };
                componentRequest.CurrentSuggestion.GATE_SuggestionPicture = pictureFile;
            }

            DB.Entry(componentRequest.CurrentSuggestion).State = EntityState.Modified;
            DB.SaveChanges();

            return new RedirectResult(Url.Action("Pending") + "#tr" + componentRequest.MaterialRequestId);
        }

        [UsageCounter]
        public ActionResult ViewSuggestion(int id = 0)
        {
            var model = DB.GATE_ComponentRequest.Find(id);
            ViewBag.CurrencySL = new SelectList(DB.Currencies.Select(x => new
            {
                text = x.ID + " - " + x.Label,
                id = x.ID
            }), "id", "text", model.CurrentSuggestion.CurrencyId);

            return View("_ViewSuggestion", model);
        }

        [UsageCounter]
        public ActionResult ViewPreviousSuggestion(int id = 0)
        {
            GATE_ComponentRequest model = DB.GATE_ComponentRequest.Find(id);
            ViewBag.CurrencySL = new SelectList(DB.Currencies.Select(x => new
            {
                text = x.ID + " - " + x.Label,
                id = x.ID
            }), "id", "text", model.CurrentSuggestion.CurrencyId);

            return View("_PreviousSuggestion", model);
        }

        public virtual FileContentResult GetSuggestionPicture(int id = 0)
        {
            var image = DB.GATE_SuggestionPicture.Where(x => x.SuggestionPictureId == id).Select(x => x.PictureData).FirstOrDefault();
            return image != null ? new FileContentResult(image, "image/jpg") : null;
        }

        public void CreateViewBagForSuggestion()
        {
            ViewBag.CurrencySL = new SelectList(DB.Currencies.Select(x => new
            {
                text = x.ID + " - " + x.Label,
                id = x.ID
            }), "id", "text");
        }

        #endregion

    }
}
