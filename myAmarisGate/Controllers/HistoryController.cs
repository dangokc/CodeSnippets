using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Amaris.Security;
using Amaris.Statistics;
using AmarisGate.Dal;
using AmarisGate.Model;

namespace AmarisGate.Controllers
{
    public class HistoryController : BootstrapBaseController
    {
        [HttpGet]
        [UsageCounter]
        public ActionResult Index()
        {
            var model = DB.GATE_MaterialRequest
                .Include(x => x.ConcernedEmployee)
                .Include(x => x.OrderedByEmployee)
                .Include(x => x.ComponentRequests.Select(y => y.GenericMaterial))
                .Include(x => x.MaterialAction.Select(y => y.Action))
                .WithTreeSecurity(LoggedUser.EmployeeId, false)
                .Where(req => req.OrderStatusId == OrderStatus.Cancelled || req.OrderStatusId == OrderStatus.Complete)
                .Select(RequestViewModel.ContructorForEF);

            return View(model);
        }
    }
}
