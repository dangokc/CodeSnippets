using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;
using Amaris.DocumentGenerator;
using Amaris.Mails;
using Amaris.Security;
using AmarisGate.Dal;
using AmarisGate.Model;

namespace AmarisGate.Controllers
{
    public class DNAController : BootstrapBaseController
    {
        // GET: DNA
        public ActionResult OrderInfoForDNA(int employeeId)
        {
            var materialRequestedForEmployee = DB.GATE_MaterialRequest
                .Include(mR => mR.Package.GenericMaterials)
                .Where(mR => mR.ConcernedEmployee.EmployeeId == employeeId)
                .Select(mR => new DNAViewModel
                {
                    ETA = mR.ExpectedDate,
                    status = mR.OrderStatus.Label,
                    statusId = (int)mR.OrderStatusId,
                    listOfMaterialFromPackage = mR.Package.GenericMaterials.Select(m => m.Label).ToList(),
                    maximumStatusId = DB.GATE_OrderStatus.Count(x => x.OrderStatusId != OrderStatus.Cancelled)
                });

            return Json(materialRequestedForEmployee, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// If an id appears more than once in the list, the Json returned by the method will contain data for each occurence of the id 
        /// </summary>
        /// <param name="employeeIds"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMaterialRequests(List<int> employeeIds)
        {
            var materialRequestedForAllEmployees = employeeIds
                .SelectMany(employeeId => DB.GATE_MaterialRequest.Include(mR => mR.Package.GenericMaterials)
                    .Where(mR => mR.ConcernedEmployee.EmployeeId == employeeId)
                    .Select(x => new
                    {
                        EmployeeId = x.ConcernedEmployeeId,
                        ETA = x.ExpectedDate.HasValue && x.ExpectedDate.Value != DateTime.MinValue ?
                         SqlFunctions.DatePart("dd", x.ExpectedDate.Value) + "/" +
                         SqlFunctions.DatePart("m", x.ExpectedDate.Value) + "/" +
                         SqlFunctions.DateName("yyyy", x.ExpectedDate.Value) : null,
                        status = x.OrderStatus.Label,
                        statusId = x.OrderStatusId,
                        listOfMaterialFromPackage = x.Package.GenericMaterials.Select(m => m.Label).ToList(),
                        maximumStatusId = DB.GATE_OrderStatus.Count(y => y.OrderStatusId != OrderStatus.Cancelled)
                    }));
            return Json(materialRequestedForAllEmployees, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MaterialRequestFor(int id)
        {
            var materialRequestedForEmployee = DB.GATE_MaterialRequest
                .Include(mR => mR.Package.GenericMaterials)
                .Where(mR => mR.ConcernedEmployee.EmployeeId == id)
                .Select(mR => new DNAViewModel
                {
                    ETA = mR.ExpectedDate,
                    status = mR.OrderStatus.Label,
                    statusId = (int)mR.OrderStatusId,
                    listOfMaterialFromPackage = mR.Package.GenericMaterials.Select(m => m.Label).ToList(),
                    maximumStatusId = DB.GATE_OrderStatus.Count(y => y.OrderStatusId != OrderStatus.Cancelled)
                })
                .FirstOrDefault() ?? new DNAViewModel
                {
                    ETA = null,
                    status = "N/A",
                    statusId = 0,
                    listOfMaterialFromPackage = null,
                    maximumStatusId = DB.GATE_OrderStatus.Count(y => y.OrderStatusId != OrderStatus.Cancelled)
                };
            return PartialView("_EmployeeRequest", materialRequestedForEmployee);
        }


        /// <summary>
        /// If an id appears more than once in the list, the Json returned by the method will contain data for each occurence of the id 
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public ActionResult GetMaterialRequests(int employeeId)
        {

            var employeeIds = DB.Employees.WithTreeSecurity(employeeId).Select(e => e.EmployeeId).ToList();

            var materialRequestedForAllEmployees = DB.GATE_MaterialRequest
                .Where(request => employeeIds.Contains(request.ConcernedEmployee.EmployeeId))
                //remove of the viewmodel because ETA is a string for api calls
                .Select(x => new
                {
                    EmployeeId = x.ConcernedEmployeeId,
                    ETA = x.ExpectedDate.HasValue && x.ExpectedDate.Value != DateTime.MinValue ?
                     SqlFunctions.DatePart("dd", x.ExpectedDate.Value) + "/" +
                     SqlFunctions.DatePart("m", x.ExpectedDate.Value) + "/" +
                     SqlFunctions.DateName("yyyy", x.ExpectedDate.Value) : null,
                    status = x.OrderStatus.Label,
                    statusId = x.OrderStatusId,
                    listOfMaterialFromPackage = x.Package.GenericMaterials.Select(m => m.Label).ToList(),
                    maximumStatusId = DB.GATE_OrderStatus.Count(y => y.OrderStatusId != OrderStatus.Cancelled)
                });
            return Json(materialRequestedForAllEmployees, JsonRequestBehavior.AllowGet);
        }
        // Generate the pdf
        public ActionResult GenerateLoanCertificatePDF(int materialRequestId)
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
            return File(ms.ToArray(), "application/pdf", "LoanCertificate.pdf");
        }

        public AmarisMail.Attachment GenerateLoanCertificateAttachmentPdf(string viewString)
        {
            Dictionary<string, object> templateModel = new Dictionary<string, object>();
            templateModel.Add("loanedArticles", viewString);

            var list = TemplateExtensions.GetApplicationTemplates("AmarisGate");
            if (!list.Any()) return new AmarisMail.Attachment("", new byte[0], "");

            var template = TemplateExtensions.GetTemplateData(list.Last().TemplateId);
            var byteArray = template.DocumentData;

            var msTemplate = Document.GenerateDocument(byteArray, templateModel);

            var ms = new FileGenerator(msTemplate.ToArray()).ConvertTextHtmlOfContentControl(msTemplate.ToArray(), new[] { "loanedArticles" });

            return new AmarisMail.Attachment("LoanCertificate.pdf", ms.ToArray(), "application/pdf");
        }
    }
}