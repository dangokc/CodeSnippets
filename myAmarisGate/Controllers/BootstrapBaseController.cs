using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amaris.XEditable.Extensions;
using AmarisGate.Dal;
using System;
using AmarisGate.Helpers;
using System.Data.Entity;
using System.IO;


namespace AmarisGate.Controllers
{
//#if !DEBUG
//    [CheckRights]
//#endif
    public class BootstrapBaseController : Controller
    {
        public const string APP_NAME = "AmarisGate";
        public const string HOME_CONTROLLER = "Home";
        protected readonly Entities DB;
        protected readonly MailHelper MailMan;
       // private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Employee _employee;

        protected Employee LoggedUser
        {
            get
            {
                if (_employee != null) return _employee;
                var userName = UserHelper.UserName();
                _employee = DB.Employees.FirstOrDefault(x => x.Login == userName);
                if (_employee == null)
                    return DB.Employees.FirstOrDefault(x => x.Login == UserHelper.RealUserName);
                return _employee;
            }

        }


        protected BootstrapBaseController()
        {
            DB = new Entities();
            MailMan = new MailHelper(this, DB, LoggedUser.EmployeeId, APP_NAME);
            ViewBag.Employee = LoggedUser;
            ViewBag.Picture = "https://arp.amaris.com/aBook/Telecom/GetEmployeeThumbnail/" + LoggedUser.EmployeeId;
        }

        public ActionResult AutoCompletePeople(string start = null)
        {
            if (string.IsNullOrWhiteSpace(start) || start.Length < 3)
            {
                return Json(new { response = "An error occured : please select at least three characters." }, JsonRequestBehavior.AllowGet);
            }

            var model = DB.Employees
                .Where(r => r.Login != null && r.EmployeeStatusId != 4 && !r.Disabled)
                .Where(r => r.Login.Contains(start) || r.Lastname.Contains(start) || r.Firstname.Contains(start) || r.Trigram.Contains(start) || r.Email.Contains(start))
                .OrderBy(x => x.Lastname)
                .Select(r => new { id = r.Login, name = r.Trigram + " | " + r.Lastname + " " + r.Firstname })
                //.Distinct()
                .ToArray();
            var result = new { collection = model };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDefaultPersonForFakeAuthentication(string login = null)
        {
            if (!string.IsNullOrWhiteSpace(login))
            {
                var employee = DB.Employees
                    .FirstOrDefault(r => r.Login == login && r.EmployeeStatusId != 4 && !r.Disabled);
                if (employee != null)
                {
                    return Json(new { id = employee.Login, name = employee.Trigram + " | " + employee.Lastname + " " + employee.Firstname }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { response = "An error occured with this employee." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FakeAuthentification(string newLogin, string returnUrl)
        {
            UserHelper.Faker(newLogin);
            return RedirectToAction("Pending", "Home");
        }

        protected ActionResult XEditableUpdate<T>(DbSet<T> set, int pk, string name, string value) where T : class
        {
            try
            {
                set.XEditableUpdate(pk, name, value);
                DB.SaveChanges();
            }
            catch (Exception e)
            {
                Response.StatusCode = 400;
                return Content("Error: " + e.Message);
            }
            return Content("Saved");
        }

        protected JsonResult XEditableList<T>(IEnumerable<T> data, Func<T, object> valueSelector, Func<T, string> textSelector)
        {
            return Json(data.ToXEditableList(valueSelector, textSelector), JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            DB.Dispose();
            base.Dispose(disposing);
        }

        protected string RenderViewToString(string viewName, object model, TempDataDictionary dictionary = null)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            if (dictionary == null)
                dictionary = new TempDataDictionary();

            var viewData = new ViewDataDictionary(model);

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, viewData, dictionary, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
