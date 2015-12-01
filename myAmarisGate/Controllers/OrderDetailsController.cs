using System;
using System.Linq;
using System.Web.Mvc;
using Amaris.Statistics;
using AmarisGate.Dal;
using AmarisGate.Model;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using Amaris.DocumentGenerator;
using Amaris.Mails;
using Amaris.Table.Filter.Linq;
using AmarisGate.Extensions;
using AmarisGate.Model.OrderDetails;
using Developpez.Dotnet.Collections;
using Microsoft.Ajax.Utilities;
using AmarisGate.Helpers;


namespace AmarisGate.Controllers
{
    public class OrderDetailsController : BootstrapBaseController
    {

        [HttpGet]
        [UsageCounter]
        public ActionResult Index(int id)
        {
            IEnumerable<int> listOfNullComponentMaterialSuggestion = DB.GATE_ComponentRequest
                                                                        .Where(x => x.MaterialRequestId == id && x.MaterialSuggestedId == null)
                                                                        .Select(x => x.ComponentId)
                                                                        .ToList();

            foreach (int compId in listOfNullComponentMaterialSuggestion)
            {
                // Create new GATE_MaterialSuggestion
                GATE_MaterialSuggestion newSuggestion = new GATE_MaterialSuggestion
                {
                    Label = string.Empty,
                    InStock = false,
                    IsNotStandardProduct = false,
                    CurrencyId = "EUR"
                };
                DB.GATE_MaterialSuggestion.Add(newSuggestion);
                DB.SaveChanges();

                // Update GATE_ComponentRequest
                var comRequest = DB.GATE_ComponentRequest.Find(compId);
                comRequest.MaterialSuggestedId = newSuggestion.MaterialSuggestionId;
                DB.SaveChanges();
            }

            var model = DB.GATE_MaterialRequest
                .Include(x => x.ConcernedEmployee)
                .Where(x => x.MaterialRequestId == id)
                .Select(OrderDetailsModel.ContructorForEF)
                .First();

            model.Components.ForEach(x => x.Order = model);
            return View("Index", model);
        }

        #region Component Statuses

        [HttpPost]
        [UsageCounter]
        public ActionResult ConfirmProposal(int id)
        {
            try
            {
                GATE_ComponentRequest item = DB.GATE_ComponentRequest
                    .Include(x => x.CurrentSuggestion)
                    .FirstOrDefault(x => x.ComponentId == id);
                if (!item.InStock)
                {
                    if (item.CurrentSuggestion.Price == null)
                    { return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Before Validation you must set a price for the material"); }

                    else if (item.CurrentSuggestion.Currency == null)
                    { return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Before Validation you must set a currency"); }
                }

                item.StatusId = OrderStatus.ProductDelivery;
                UpdateMaterialRequest(item.MaterialRequestId);

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
                GATE_ComponentRequest item = DB.GATE_ComponentRequest
                    .Include(x => x.GenericMaterial)
                    .FirstOrDefault(x => x.ComponentId == id);

                item.StatusId = item.GenericMaterial.RequiresSetup ? OrderStatus.ProductSetup : OrderStatus.ProductDispatch;

                UpdateMaterialRequest(item.MaterialRequestId);

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
                GATE_ComponentRequest item = DB.GATE_ComponentRequest.Find(id);

                item.IsSetup = true;
                item.StatusId = OrderStatus.ProductDispatch;
                UpdateMaterialRequest(item.MaterialRequestId);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult ConfirmDispatch(int id)
        {
            try
            {
                GATE_ComponentRequest item = DB.GATE_ComponentRequest.Find(id);

                item.IsDispatched = true;
                item.StatusId = OrderStatus.Complete;
                UpdateMaterialRequest(item.MaterialRequestId);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Ajax and XEditable Functions

        #region setters

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateComponentRequestInStock(int pk, string name, string value)
        {
            //TODO reimplement with the new Stock tables #stock
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
            GATE_MaterialSuggestion suggestion = DB.GATE_MaterialSuggestion.Find(pk);
            List<GATE_ComponentRequest> components = suggestion.GATE_ComponentRequest1.ToList();
            DateTime suggestionDate = Convert.ToDateTime(value);
            foreach (GATE_ComponentRequest comp in components)
            {
                //TODO REFACTO TO AVOID NULL REF
                var orderDate = comp.MaterialRequest.ExpectedDate;
                if (!orderDate.HasValue || (DateTime.Compare(orderDate.Value, suggestionDate) < 0))
                {
                    comp.MaterialRequest.ExpectedDate = suggestionDate;
                }
            }

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
        public ActionResult UpdateMaterialSuggestionComment(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_MaterialSuggestion, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateOrderCurrencies(int pk, string name, string value)
        {
            GATE_MaterialRequest request = DB.GATE_MaterialRequest.Find(pk);

            foreach (GATE_ComponentRequest component in request.ComponentRequests)
            {
                component.CurrentSuggestion.CurrencyId = value;
            }

            return XEditableUpdate(DB.GATE_MaterialRequest, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateComponentRequestOwner(int pk, string name, string value)
        {
            return XEditableUpdate(DB.GATE_ComponentRequest, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public void SetSelfAsOwner(int componentId)
        {
            var component = DB.GATE_ComponentRequest.Find(componentId);
            var loggedEmployeeId = LoggedUser.EmployeeId;
            component.OwnerId = loggedEmployeeId;
            DB.SaveChanges();
        }

        [HttpPost]
        [UsageCounter]
        public void SetSelfAsOwnerForAll(int orderId)
        {
            var components = DB.GATE_ComponentRequest.Where(w => w.MaterialRequestId == orderId);
            components.ForEach(fe => fe.OwnerId = LoggedUser.EmployeeId);
            DB.SaveChanges();
        }

        [HttpPost]
        [UsageCounter]
        public void DeleteOwner(int componentId)
        {
            var component = DB.GATE_ComponentRequest.Find(componentId);
            component.OwnerId = null;
            DB.SaveChanges();
        }

        #endregion

        #region getters

        [HttpGet]
        public ActionResult GetCurrencies(string query = "")
        {
            var currenciesList = DB.Currencies.Where(curr => curr.ID.ToLower().Contains(query.ToLower()) || curr.Label.ToLower().Contains(query.ToLower()) || query.Trim().Equals(string.Empty)).Select(aCurrency => new { id = aCurrency.ID, text = aCurrency.ID }).ToList();
            return Json(currenciesList, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region partial reload
        //[HttpPost]
        public ActionResult ReloadComponentTable(int id)
        {
            var model = DB.GATE_MaterialRequest
               .Where(x => x.MaterialRequestId == id)
               .Select(OrderDetailsModel.ContructorForEF)
               .First();
            model.Components.ForEach(x => x.Order = model);
            return PartialView("_ComponentsTable", model);
        }

        public ActionResult ReloadComponentRow(int id)
        {
            OrderDetailsModel request = DB.GATE_ComponentRequest
                .Where(x => x.ComponentId == id)
                .Select(x => x.MaterialRequest)
                .Select(OrderDetailsModel.ContructorForEF)
                .First();
            request.Components.ForEach(x => x.Order = request);
            ComponentRowModel model = request.Components.First(x => x.Component.ComponentId == id);
            return PartialView("_ComponentRow", model);
        }

        

        #endregion

        #endregion

        #region Aux Methods

        private void UpdateMaterialRequest(int reqId)
        {
            var request = DB.GATE_MaterialRequest
                .Include(x => x.ComponentRequests)
                .First(x => x.MaterialRequestId == reqId);
            var oldOrderStatus = request.OrderStatusId;
            var minStatus = request.ComponentRequests.Min(comp => comp.StatusId);
            request.OrderStatusId = minStatus;
            DB.SaveChanges();


            if (oldOrderStatus != minStatus)
            {
                MailMan.SendAlertMails(request);
                if (minStatus == OrderStatus.Complete)
                {
                    //Start the loaning process -- not using the mailhelper 
                    LoanArticlesCertificateMail(request.MaterialRequestId);
                }
            }
        }

        #endregion

        #region Clean Functions

        public void FixComponentSuggestions()
        {
            IEnumerable<GATE_ComponentRequest> comps = DB.GATE_ComponentRequest.Where(com => com.CurrentSuggestion == null);
            foreach (GATE_ComponentRequest theComponent in comps)
            {
                GATE_MaterialSuggestion newSuggestion = new GATE_MaterialSuggestion
                {
                    InStock = false,
                    IsNotStandardProduct = false,
                    Label = string.Empty,
                    CurrencyId = "EUR"
                };

                DB.GATE_MaterialSuggestion.Add(newSuggestion);
                theComponent.CurrentSuggestion = newSuggestion;
            }
            DB.SaveChanges();
        }

        #endregion

        #region ModalSetProposal

        public ActionResult SetProposal(int componentId, int materialRequestId)
        {
            var viewModel = new SetProposalViewModel
            {
                ComponentId = componentId,
                MaterialRequestId = materialRequestId,
                EmployeeId = (Session["SelectedEmployeeId"] != null) ? (int)Session["SelectedEmployeeId"] : 0
            };
            return PartialView("_SetProposal", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitProposal(SetProposalViewModel viewModel)
        {
            var existingEmployee = DB.StockHistories
                                .Where(x => x.UserId == viewModel.EmployeeId && x.EndDate == null)
                                .OrderByDescending(x => x.StockHistoryId)
                                .FirstOrDefault();


            if (existingEmployee != null)
            {
                // Update Stock History if material is already in
                existingEmployee.EndDate = DateTime.Today;
                existingEmployee.UpdatedDate = DateTime.Today;
                existingEmployee.UpdatedById = LoggedUser.EmployeeId;

                StockHistory stockHistory = new StockHistory();
                stockHistory.StockId = existingEmployee.StockId;
                stockHistory.UserId = viewModel.EmployeeId;
                stockHistory.StartDate = DateTime.Today;
                stockHistory.EndDate = null;
                stockHistory.CreatedDate = DateTime.Today;
                stockHistory.CreatedById = LoggedUser.EmployeeId;
                stockHistory.UpdatedDate = null;
                stockHistory.UpdatedById = null;
                DB.StockHistories.Add(stockHistory);


                // Update GATE_ComponentRequest
                var componentRequest = DB.GATE_ComponentRequest.Find(viewModel.ComponentId);
                componentRequest.StockHistoryId = existingEmployee.StockHistoryId;
                DB.SaveChanges();

            }
            else
            {
                if (viewModel.EmployeeId != 0)
                {
                    // Add material into Stock
                    Stock stock = new Stock();
                    StockHelper stockService = new StockHelper(DB, LoggedUser);
                    string companyCodeName = stockService.FindCompanyCodeNameFromEmployee(viewModel.EmployeeId);
                    string materialCode = stockService.FindMaterialCodeFromComponentId(viewModel.ComponentId);

                    int companyId = DB.Companies.Where(x => x.CompanyCodeName == companyCodeName).Select(x => x.ID).FirstOrDefault();
                    int materialId = DB.GenericMaterials
                                        .Where(x => x.MaterialCode == materialCode)
                                        .Select(x => x.GenericMaterialId)
                                        .FirstOrDefault();
                    int seqnember = stockService.GetNextSeqNumber(companyId, materialId);
                    string productCode = stockService.GenerateProductCode(companyCodeName, materialCode, seqnember);

                    stock.ProductCode = productCode;
                    stock.CompanyId = companyId;
                    stock.GenericMaterialId = materialId;
                    stock.SeqNumber = seqnember;
                    stock.Note = null;
                    stock.StockStatusId = StockStatus.InStock;

                    DB.Stocks.Add(stock);
                    DB.SaveChanges();

                    // Update Stock History for previous employee 
                    StockHistory previousEmployee = new StockHistory();

                    previousEmployee.StockId = stock.StockId;
                    previousEmployee.UserId = viewModel.EmployeeId;
                    previousEmployee.StartDate = DateTime.Today;
                    previousEmployee.EndDate = DateTime.Today;
                    previousEmployee.CreatedById = LoggedUser.EmployeeId;
                    previousEmployee.CreatedDate = DateTime.Today;
                    previousEmployee.UpdatedDate = DateTime.Today;
                    previousEmployee.UpdatedById = LoggedUser.EmployeeId;

                    DB.StockHistories.Add(previousEmployee);
                    DB.SaveChanges();

                    // Update GATE_ComponentRequest
                    var componentRequest = DB.GATE_ComponentRequest.Find(viewModel.ComponentId);
                    componentRequest.StockHistoryId = previousEmployee.StockHistoryId;

                    // Update Stock History for new employee 
                    StockHistory newEmployee = new StockHistory();
                    newEmployee.StockId = stock.StockId;
                    newEmployee.UserId = DB.GATE_MaterialRequest
                                            .Where(x => x.MaterialRequestId == viewModel.MaterialRequestId)
                                            .Select(x => x.ConcernedEmployeeId)
                                            .FirstOrDefault();
                    newEmployee.StartDate = DateTime.Today;
                    newEmployee.EndDate = null;
                    newEmployee.CreatedDate = DateTime.Today;
                    newEmployee.CreatedById = LoggedUser.EmployeeId;
                    newEmployee.UpdatedDate = null;
                    newEmployee.UpdatedById = null;

                    DB.StockHistories.Add(newEmployee);
                    DB.SaveChanges();
                }
                // User select a material from Stock, not from an employee
                else
                {
                    // Add material into Stock
                    Stock stock = new Stock();
                    StockHelper stockService = new StockHelper(DB, LoggedUser);

                    string companyCodeName = DB.GATE_MaterialRequest
                                        .Where(x => x.MaterialRequestId == viewModel.MaterialRequestId)
                                        .Select(x => x.ConcernedEmployee.Company.CompanyCodeName)
                                        .FirstOrDefault();

                    string materialCode = stockService.FindMaterialCodeFromComponentId(viewModel.ComponentId);

                    int companyId = DB.Companies.Where(x => x.CompanyCodeName == companyCodeName).Select(x => x.ID).FirstOrDefault();
                    int materialId = DB.GenericMaterials
                                        .Where(x => x.MaterialCode == materialCode)
                                        .Select(x => x.GenericMaterialId)
                                        .FirstOrDefault();
                    int seqnember = stockService.GetNextSeqNumber(companyId, materialId);
                    string productCode = stockService.GenerateProductCode(companyCodeName, materialCode, seqnember);

                    stock.ProductCode = productCode;
                    stock.CompanyId = companyId;
                    stock.GenericMaterialId = materialId;
                    stock.SeqNumber = seqnember;
                    stock.Note = null;
                    stock.StockStatusId = StockStatus.InStock;

                    DB.Stocks.Add(stock);
                    DB.SaveChanges();

                    // Update Stock History. It is same to indicate the material from Stock
                    StockHistory previousEmployee = new StockHistory();

                    previousEmployee.StockId = stock.StockId;
                    previousEmployee.UserId = DB.GATE_MaterialRequest
                                            .Where(x => x.MaterialRequestId == viewModel.MaterialRequestId)
                                            .Select(x => x.ConcernedEmployeeId)
                                            .FirstOrDefault();
                    previousEmployee.StartDate = DateTime.Today;
                    previousEmployee.EndDate = DateTime.Today;
                    previousEmployee.CreatedById = LoggedUser.EmployeeId;
                    previousEmployee.CreatedDate = DateTime.Today;
                    previousEmployee.UpdatedDate = DateTime.Today;
                    previousEmployee.UpdatedById = LoggedUser.EmployeeId;

                    DB.StockHistories.Add(previousEmployee);
                    DB.SaveChanges();

                    // Update GATE_ComponentRequest
                    var componentRequest = DB.GATE_ComponentRequest.Find(viewModel.ComponentId);
                    componentRequest.StockHistoryId = previousEmployee.StockHistoryId;

                    // Update Stock History for new employee 
                    StockHistory newEmployee = new StockHistory();
                    newEmployee.StockId = stock.StockId;
                    newEmployee.UserId = DB.GATE_MaterialRequest
                                            .Where(x => x.MaterialRequestId == viewModel.MaterialRequestId)
                                            .Select(x => x.ConcernedEmployeeId)
                                            .FirstOrDefault();
                    newEmployee.StartDate = DateTime.Today;
                    newEmployee.EndDate = null;
                    newEmployee.CreatedDate = DateTime.Today;
                    newEmployee.CreatedById = LoggedUser.EmployeeId;
                    newEmployee.UpdatedDate = null;
                    newEmployee.UpdatedById = null;

                    DB.StockHistories.Add(newEmployee);
                    DB.SaveChanges();
                }

            }

            Session["SelectedEmployeeId"] = viewModel.EmployeeId;

            return ConfirmDelivery(viewModel.ComponentId);
        }

        #endregion

        #region Mails
        /// <summary>
        /// also called Loan agreement request
        /// </summary>
        /// <param name="materialRequestId"></param>
        public void LoanArticlesCertificateMail(int materialRequestId)
        {
            var listLoans = DB.GenericMaterials.Where(material => material.LoanRequested && material.ComponentRequests.Any(request => request.MaterialRequestId == materialRequestId)).ToList();
            if (!listLoans.IsNullOrEmpty())
            {
                var concernedEmployee = DB.Employees.SingleOrDefault(employee => employee.GATE_MaterialRequest.Any(request => request.MaterialRequestId == materialRequestId));
                var concernedManager = DB.Employees.SingleOrDefault(employee => employee.Managees.Any(employee1 => employee1.EmployeeId == concernedEmployee.EmployeeId));
                var materialRequestOffice = DB.GATE_MaterialRequest.Find(materialRequestId) == null ? null : DB.GATE_MaterialRequest.Find(materialRequestId).Office;
                var concernedLOM = materialRequestOffice == null ? null : DB.Employees.SingleOrDefault(employee => employee.LocalOfficeManaged.Any(office => office.OfficeId == materialRequestOffice.OfficeId));

                var ListOfItems = RenderViewToString("_LoanArticles", listLoans);

                var LinkToLoanValidation = RenderViewToString("_LoanArticlesCertificateMail", materialRequestId);

                IEnumerable<AmarisMail.Attachment> loanCertificate = GenerateLoanCertificateAttachment(ListOfItems).Yield();

                DB.SendMail("LoanArticlesCertificate", new
                {
                    employeeEmail = concernedEmployee != null ? concernedEmployee.Email : "",
                    managerEmail = concernedManager != null ? concernedManager.Email : "",
                    LOMEmail = concernedLOM != null ? concernedLOM.Email : "",
                    EmployeeFirstName = concernedEmployee != null ? concernedEmployee.Firstname : "",
                    EmployeeLastName = concernedEmployee != null ? concernedEmployee.Lastname : "",
                    ListOfItems = ListOfItems,
                    LinkToLoanValidation = LinkToLoanValidation
                }, null, "en", "AmarisGate", loanCertificate);
            }
        }

        [HttpGet]
        public ActionResult ValidateLoans(int materialRequestId)
        {
            var listLoans = DB.GenericMaterials.Where(material => material.LoanRequested && material.ComponentRequests.Any(request => request.MaterialRequestId == materialRequestId)).ToList();
            var concernedEmployee = DB.Employees.SingleOrDefault(employee => employee.GATE_MaterialRequest.Any(request => request.MaterialRequestId == materialRequestId));
            var concernedManager = DB.Employees.SingleOrDefault(employee => employee.Managees.Any(employee1 => employee1.EmployeeId == concernedEmployee.EmployeeId));
            var materialRequestOffice = DB.GATE_MaterialRequest.Find(materialRequestId) == null ? null : DB.GATE_MaterialRequest.Find(materialRequestId).Office;
            var concernedLOM = materialRequestOffice == null ? null : DB.Employees.SingleOrDefault(employee => employee.LocalOfficeManaged.Any(office => office.OfficeId == materialRequestOffice.OfficeId));

            var ListOfItems = RenderViewToString("_LoanArticles", listLoans);
            var materialRequest = DB.GATE_MaterialRequest.Find(materialRequestId);
            DB.Entry(materialRequest).State = EntityState.Modified;
            DB.GATE_MaterialRequest.Find(materialRequestId).LoanValidated = true;
            DB.SaveChanges();

            DB.SendMail("LoanConfirmation", new
            {
                employeeEmail = concernedEmployee != null ? concernedEmployee.Email : "",
                managerEmail = concernedManager != null ? concernedManager.Email : "",
                LOMEmail = concernedLOM != null ? concernedLOM.Email : "",
                EmployeeFirstName = concernedEmployee != null ? concernedEmployee.Firstname : "",
                EmployeeLastName = concernedEmployee != null ? concernedEmployee.Lastname : "",
                ListOfItems = ListOfItems
            }, null, "en", "AmarisGate");

            return View("LoanValidated");
        }

        public ActionResult GenerateLoanCertificate(int materialRequestId)
        {
            var listLoans = DB.GenericMaterials.Where(material => material.LoanRequested && material.ComponentRequests.Any(request => request.MaterialRequestId == materialRequestId)).ToList();
            var viewString = RenderViewToString("_LoanArticles", listLoans);
            Dictionary<string, object> templateModel = new Dictionary<string, object>();
            templateModel.Add("loanedArticles", viewString);

            var list = TemplateExtensions.GetApplicationTemplates("AmarisGate");
            if (!list.Any()) return HttpNotFound();

            var template = TemplateExtensions.GetTemplateData(list.Last().TemplateId);
            var byteArray = template.DocumentData;

            var msTemplate = Document.GenerateDocument(byteArray, templateModel);

            var ms = new FileGenerator(msTemplate.ToArray()).ConvertTextHtmlOfContentControl(msTemplate.ToArray(), new[] { "loanedArticles" });
            //return the file
            return File(ms.ToArray(), "application/msword", "LoanCertificate.docx");
        }

        public AmarisMail.Attachment GenerateLoanCertificateAttachment(string viewString)
        {
            Dictionary<string, object> templateModel = new Dictionary<string, object>();
            templateModel.Add("loanedArticles", viewString);

            var list = TemplateExtensions.GetApplicationTemplates("AmarisGate");
            if (!list.Any()) return new AmarisMail.Attachment("", new byte[0], "");

            var template = TemplateExtensions.GetTemplateData(list.Last().TemplateId);
            var byteArray = template.DocumentData;

            var msTemplate = Document.GenerateDocument(byteArray, templateModel);

            var ms = new FileGenerator(msTemplate.ToArray()).ConvertTextHtmlOfContentControl(msTemplate.ToArray(), new[] { "loanedArticles" });

            return new AmarisMail.Attachment("LoanCertificate.docx", ms.ToArray(), "application/msword");
        }

        #endregion

    }
}
