using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Amaris.Application.Attributes;
using CountryStrategy.Models;
using Amaris.Statistics;
using Amaris.XEditable;
using CountryStrategy.Models.Partial;
using CountryStrategy.Models.Helper;

namespace CountryStrategy.Controllers
{
    [Beta]
    public class HomeController : AmarisController
    {
        #region 1. Main Action Method for render View

        #region 1.1 Method for Index View
        [UsageCounter]
        public ActionResult Index()
        {
            
            var model = new OcpModelView();

            model.LstOpeningCountryPriority = Db.GetAllPriority(Log).DefaultOrder();

            if (model.LstOpeningCountryPriority == null)
            {
                Error("Something went wrong. Please consult Application Logs.");
            }

            return View(model);
        }

        public ActionResult GetTableList()
        {

            var model = new OcpModelView();

            model.LstOpeningCountryPriority = Db.GetAllPriority(Log).OrderByPriorityOrder();

            if (model.LstOpeningCountryPriority == null)
            {
                Error("Something went wrong. Please consult Application Logs.");
            }

            return PartialView("_TableList", model);
        }

        [HttpPost]
        public ActionResult Index(string keyword, string lstPriorityTypeId, string lstDeadlineTypeId)
        {
            // Split string of lstPriorityTypeId to array of PriorityTypeId and convert to long datatype
            var lstPt = new List<long>();
            if (!string.IsNullOrEmpty(lstPriorityTypeId))
            {
                string[] lstPTid = lstPriorityTypeId.Split(new[] { "," }, StringSplitOptions.None);
                lstPt.AddRange(lstPTid.Select(item => Convert.ToInt64(item)));
            }

            // Split string of lstDeadlineTypeId to array of DeadlineTypeId and convert to long datatype
            var lstDt = new List<long>();
            if (!string.IsNullOrEmpty(lstDeadlineTypeId))
            {
                string[] lstDTid = lstDeadlineTypeId.Split(new[] { "," }, StringSplitOptions.None);
                lstDt.AddRange(lstDTid.Select(item => Convert.ToInt64(item)));
            }

            // declare list of opening country with priority
            var model = new OcpModelView();


            // check if all box is empty, display all   
            if (string.IsNullOrEmpty(keyword) && string.IsNullOrEmpty(lstPriorityTypeId) && string.IsNullOrEmpty(lstDeadlineTypeId))
            {
                model.LstOpeningCountryPriority = Db.GetAllPriority(Log).DefaultOrder();
            }
            else
            {
                // call search method            
                model.LstOpeningCountryPriority = Db.GetPriorityWithFilter(Log, keyword, lstPt, lstDt).DefaultOrder();
            }

            return PartialView("_TableList", model);
        }
        
        #endregion

        #region 1.2 Method for AddForm View
        //AddForm for Country Opening Priority
        public ActionResult AddOcpModal()
        {
            return PartialView("_AddForm");
        }        

        //Add & Edit Save method for Country Opening Priority
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOcpModal(OcpModelView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var item =
                        Db.Priorities.FirstOrDefault(
                            x => x.PriorityId == model.OpeningCountryPriority.PriorityId);
                    //Edit
                    if (item != null)
                    {
                        //question.PriorityId = model.OpeningCountryPriority.PriorityId;
                        //question.DeadlineTypeId = model.OpeningCountryDeadlineType.DeadlineTypeId;
                        //question.CountryId = model.OpeningCountryPriority.CountryId;
                        //question.DeadlineDate = model.OpeningCountryPriority.DeadlineDate;

                        //question.DirectorId = model.OpeningCountryPriority.DirectorId;
                        //question.AFMId = model.OpeningCountryPriority.AFMId;
                        //question.CorpDevId = model.OpeningCountryPriority.CorpDevId;
                        //question.Comment = model.OpeningCountryPriority.Comment;
                        //Db.SaveChanges();
                    }
                    //Add
                    else
                    {
                        Db.Priorities.Add(new Priority()
                        {
                            PriorityTypeId = model.OpeningCountryPriority.PriorityTypeId,
                            DeadlineTypeId = model.OpeningCountryPriority.DeadlineTypeId,
                            CountryId = model.OpeningCountryPriority.CountryId,
                            DeadlineDate = model.OpeningCountryPriority.DeadlineDate,
                            DirectorId = model.OpeningCountryPriority.DirectorId,
                            AFMId = model.OpeningCountryPriority.AFMId,
                            CorpDevId = model.OpeningCountryPriority.CorpDevId,
                            Comment = model.OpeningCountryPriority.Comment,
                        });
                        Db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    Error(ex.Message);
                }
            }
            else
            {
                Error("All the inputs are not field well. Please check and try again.");
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #endregion


        #region 2. Support Method

        #region 2.1 Method Get data for Select2 & XEditable
        // Get data for Filter Select2 & XEditable PriorityType 
        public ActionResult GetPriorityTypeList(string query = "", int id = 0)
        {
            // set valid PriorityType (IsDeleted = False)
            const bool isDeleted = false;
            // Get all valid PriorityType by GetAllPriorityType() method and search on that list
            var lstPt = Db.GetAllPriorityType(Log, isDeleted)
                .Where(x => 
                    (id != 0 && x.PriorityTypeId == id )
                    || 
                    (query == String.Empty || x.PriorityTypeName.ToLower().Contains(query.ToLower())))
                .OrderBy(x => x.LevelRole)
                .Select(item => new
                {
                    id = item.PriorityTypeId,
                    text = item.PriorityTypeName
                })
                .ToList();
            return Json(lstPt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPriorityOrderList(string query = "", int id = 0)
        {
            var lstPt = Db.GetAllPriority(Log) 
                .Select(x => new
                {
                    id = x.PriorityOrder,
                    text = x.PriorityOrder
                })                
                .ToList();
            return Json(lstPt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SortByPriorityOrder()
        {
            var model = new OcpModelView();
            model.LstOpeningCountryPriority = Db.GetAllPriority(Log).OrderByPriorityOrder();
            return PartialView("_TableList", model);
        }

        public ActionResult SortByPriority(string query = "", int id = 0)
        {
            var model = new OcpModelView();
            model.LstOpeningCountryPriority = Db.GetAllPriority(Log).OrderByPriority();
            return PartialView("_TableList", model);
        }

        public ActionResult SortByCountry(string query = "", int id = 0)
        {
            var model = new OcpModelView(); 
            model.LstOpeningCountryPriority = Db.GetAllCountry(Log).OrderByCountry();
            return PartialView("_TableList", model);
        }

        public ActionResult SortByDeadline(string query = "", int id = 0)
        {
            var model = new OcpModelView();
            model.LstOpeningCountryPriority = Db.GetAllPriority(Log).OrderByDeadline();
            return PartialView("_TableList", model);
        }

        // Get data for Filter Select2 & XEditable DeadlineType 
        public ActionResult GetDeadlineTypeList(string query = "", int id = 0)
        {
            var repoDeadlineType = new DeadlineTypeRepository();
            // Set valid DeadlineType (IsDeleted = False)
            const bool isDeleted = false;
            // Get all valid DeadlineType by GetAllDeadlineType() method and search on that list
            var lstPt = repoDeadlineType.GetAllDeadlineType(Db, Log, isDeleted)
                .Where(x => query == String.Empty || x.DeadlineTypeName.ToLower().Contains(query.ToLower()))
                .Select(item => new
                {
                    id = item.DeadlineTypeId,
                    text = item.DeadlineTypeName
                }).ToList();
            return Json(lstPt, JsonRequestBehavior.AllowGet);
        }

        // Get data for Filter Select2 & XEditable Employee  
        public ActionResult GetEmployeeList(string query = "", string id = "", bool multiple = false)
        {
            // Get one Employee if id exist (for edit featured)
            if (!string.IsNullOrEmpty(id))
            {
                if (multiple)
                {
                    var idsArray = id.Split(',').Select(Int32.Parse).ToList();
                    var listById = Db.Employee
                                        .Where(x => idsArray.Contains(x.EmployeeId))
                                        .Select(x => new
                                        {
                                            id = x.EmployeeId,
                                            text = x.Lastname.ToUpper() + " " + x.Firstname
                                        })
                                        .ToList();

                    return Json(listById, JsonRequestBehavior.AllowGet);
                }

                var name = Db.Employee.Where(x => x.EmployeeId == int.Parse(id)).Select(x => string.Format("{0} {1}",           
                    x.Lastname, x.Firstname)).FirstOrDefault();
                
                return Json(new XEditableItem { id = id, text = name }, JsonRequestBehavior.AllowGet);
            }

            // Get all valid Employee and search on that list
            var lstEmp = Db.Employee
               .Where(emp => !emp.Disabled)
               .Where(x => (x.Lastname + " " + x.Firstname).ToLower().Contains(query) || x.Email.ToLower().Contains(query))
               .OrderBy(emp => emp.Lastname + emp.Firstname)
               .Take(20) // Max Number of item you want to show
                .Select(item => new
                {
                    id = item.EmployeeId,
                    text = item.Lastname + " " + item.Firstname
                }).ToList();
            return Json(lstEmp, JsonRequestBehavior.AllowGet);
        }

        // Get data for Filter Select2 & XEditable Country 
        public ActionResult GetCountryList(string query = "", int id = 0)
        {
            // Get all valid PriorityType by GetAllPriorityType() method and search on that list
            var lstEmp = Db.Countries
                .Where(x => query == String.Empty || x.Label.ToLower().Contains(query.ToLower()))
                .OrderBy(x => x.Label)
                .Take(20) // Max Number of item you want to show
                .Select(item => new
                {
                    id = item.ID,
                    text = item.Label
                })
                .ToList();
            return Json(lstEmp, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 2.2 Method Update data for XEditable
        public ActionResult UpdateOrderOnPriority(int pk, string name, string value)
        {
            this.Db.UpdatePriorityOrder(pk, int.Parse(value));
            return JsonSuccess("Update PriorityOrder sucessfully!");
        }
        // Set data for XEditable Priority 
        public ActionResult UpdatePriority(int pk, string name, string value)
        {
            return XEditableUpdate(Db.Priorities, pk, name, value);
        }
        
        public ActionResult InsertAfmPriority(int pk, string name, List<int> value)
        {
            this.Db.UpdateAFMPriority(pk, value);
            return JsonSuccess("Update AfmPriority sucessfully!");
        }
        public ActionResult InsertDirectorPriority(int pk, string name, List<int> value)
        {
            this.Db.UpdateDirectorPriority(pk, value);
            return JsonSuccess("Update DirectorPriority sucessfully!");
        }
        public ActionResult InsertCorpDevPriority(int pk, string name, List<int> value)
        {
            this.Db.UpdateCorpDevPriority(pk, value);
            return JsonSuccess("Update CorpDevPriority sucessfully!");
        }

        #endregion

        #endregion
    }

}