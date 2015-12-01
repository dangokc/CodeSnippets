using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Amaris.Security.RoleProvider;
using AmarisGate.Dal;
using AmarisGate.Model.Stock;
using Elmah;


namespace AmarisGate.Controllers
{
    public class StockController : BootstrapBaseController
    {
        [CheckRights]
        public ActionResult Index()
        {
            //TODO think of a simpler ViewModel where every property is used in the View
            //MaterialScreenCodeModel model = new MaterialScreenCodeModel();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchScreenPassword(int stockId)
        {
            //TODO Secure the data
            var productInStock = DB.Stocks
                                .Include(x => x.ProductPassword)
                                .Include(x => x.StockHistory.Select(sh => sh.User))
                                .FirstOrDefault(x => x.StockId == stockId);

            if (productInStock == null)
                return HttpNotFound();

            int? newEmployeeArrival = productInStock.StockHistory
                                                    .Where(x => x.EndDate == null && x.StockId == stockId)
                                                    .Select(x => x.UserId)
                                                    .FirstOrDefault();

            string assignedEmployee = "";
            DateTime? assignedDateOfArrival = null;

            if (newEmployeeArrival != 0)
            {
                string lastname = DB.Employees
                                    .Where(x => x.EmployeeId == newEmployeeArrival)
                                    .Select(x => x.Lastname)
                                    .FirstOrDefault();

                string firstname = DB.Employees
                                    .Where(x => x.EmployeeId == newEmployeeArrival)
                                    .Select(x => x.Firstname)
                                    .FirstOrDefault();

                assignedEmployee = lastname + " " + firstname;

                assignedDateOfArrival = DB.Employees
                                                .Where(x => x.EmployeeId == newEmployeeArrival)
                                                .Select(x => x.EntryDate)
                                                .FirstOrDefault();
            }

            var pwdViewModels = new MaterialScreenCodeModel
            {
                MaterialScreenCode = productInStock.ProductCode,
                Password = GetPassword(productInStock),
                LastUserId = GetLastUser(productInStock),
                LoggedEmpId = LoggedUser.EmployeeId,
                NewArrival = assignedEmployee,
                DateOfArrival = assignedDateOfArrival,
            };

            return PartialView("_SearchScreenPasswordResult", pwdViewModels);
        }

        public ActionResult GetCompanyList(string query)
        {
            var lstCode = DB.Companies
                            .Where(x => string.IsNullOrEmpty(query) || x.CompanyCodeName.ToLower().Contains(query.ToLower()))
                            .OrderBy(x => x.CompanyCodeName)
                            .Take(15)
                            .Select(item => new
                            {
                                id = item.ID,
                                text = item.CompanyCodeName
                            })
                            .ToList();

            return Json(lstCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterialCode(string query)
        {
            var lstMaterialCode = DB.GenericMaterials
                                    .Where(x => string.IsNullOrEmpty(query) || x.Label.ToLower().Contains(query.ToLower()))
                                    .OrderBy(x => x.MaterialCode)
                                    .Take(15)
                                    .Select(item => new
                                    {
                                        id = item.GenericMaterialId,
                                        text = item.Label
                                    })
                                    .ToList();

            return Json(lstMaterialCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDeviceId(string query)
        {
            var lstMaterialCode = DB.Stocks
                                    .Where(x => string.IsNullOrEmpty(query) || x.ProductCode.ToLower().Contains(query.ToLower()))
                                    .OrderBy(x => x.ProductCode)
                                    .Take(15)
                                    .Select(item => new
                                    {
                                        id = item.StockId,
                                        text = item.ProductCode
                                    })
                                    .ToList();

            return Json(lstMaterialCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Stock stock)
        {
            bool isSuccess = false;
            bool isRequiredSetup = false;
            string password = string.Empty;

            if (ModelState.IsValid)
            {
                //TODO Improve this : decrease number of calls to the DB
                var company = DB.Companies.Find(stock.CompanyId);
                var material = DB.GenericMaterials.Find(stock.GenericMaterialId);
                var seqNumber = GetNextSeqNumber(stock.CompanyId, stock.GenericMaterialId);

                //Fill stock entity properties
                stock.SeqNumber = seqNumber;
                stock.StockStatusId = StockStatus.InStock;
                stock.ProductCode = GenerateProductCode(company.CompanyCodeName, material.MaterialCode, seqNumber);
                try
                {
                    DB.Stocks.Add(stock);
                    DB.SaveChanges();

                    //Create password entity
                    isRequiredSetup = DB.GenericMaterials
                                            .Where(x => x.GenericMaterialId == stock.GenericMaterialId)
                                            .Select(x => x.RequiresSetup)
                                            .FirstOrDefault();
                    if (isRequiredSetup)
                    {
                        password = CreateRandomPassword(6);
                        var pass = new ProductPassword
                        {
                            Password = password,
                            StockId = stock.StockId
                        };
                        DB.ProductPasswords.Add(pass);
                        DB.SaveChanges();
                    }
                    isSuccess = true;

                    //Create history ?
                    //TODO return details view of the newly created product in the stock
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                }
            }

            return Json(new { success = isSuccess, isRequiredPassword = isRequiredSetup, productCode = stock.ProductCode, password = password }, JsonRequestBehavior.AllowGet);
        }

        public bool UpdatePassword(string productcode, string password)
        {
            Stock stock = DB.Stocks
                            .Where(x => x.ProductCode == productcode)
                            .ToList()
                            .FirstOrDefault();
            if (stock == null)
            {
                return false;
            }

            try
            {
                ProductPassword productPasswordToUpdate = DB.ProductPasswords
                                            .Where(x => x.StockId == stock.StockId)
                                            .ToList()
                                            .First();
                productPasswordToUpdate.Password = password;
                DB.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region private Methods (Utilities)
        private static string CreateRandomPassword(int passwordLength)
        {
            var _letters = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";
            var randNum = new Random();
            var chars = new char[passwordLength];
            var letterLength = _letters.Length;
            for (var i = 0; i < passwordLength; i++)
            {
                chars[i] = _letters[(int)((letterLength) * (randNum.NextDouble()))];
            }

            return new string(chars);
        }

        private int GetNextSeqNumber(int companyId, int materialId)
        {
            var list = DB.Stocks.Where(x => x.CompanyId == companyId && x.GenericMaterialId == materialId);

            if (list.Any())
                return list.Max(x => x.SeqNumber) + 1;

            return 1;
        }

        private string GetPassword(Stock productInStock)
        {
            // TODO when pass crypted and salted update this method to get the password uncrypted
            var productPassword = productInStock.ProductPassword.FirstOrDefault();

            if (productPassword != null)
                return productPassword.Password;

            bool isRequiedSetup = DB.GenericMaterials
                                    .Where(x => x.GenericMaterialId == productInStock.GenericMaterialId)
                                    .Select(x => x.RequiresSetup)
                                    .FirstOrDefault();
            if (!isRequiedSetup)
            {
                return null;
            }
            else
            {
                var p = new ProductPassword
                {
                    StockId = productInStock.StockId,
                    Password = CreateRandomPassword(6),
                };
                DB.ProductPasswords.Add(p);
                DB.SaveChanges();
                return p.Password;
            }
        }


        private int? GetLastUser(Stock productInStock)
        {
            var stockHistory = productInStock.StockHistory;

            if (stockHistory.Any())
            {
                //TODO test start and end dates?
                var lastUser = stockHistory.OrderByDescending(x => x.StockHistoryId).FirstOrDefault().UserId;
                return lastUser;
            }

            return null;
        }

        private string GenerateProductCode(string companyCodeName, string materialLabel, int seqNumber)
        {
            return string.Format("{0}_{1}_{2}", companyCodeName, materialLabel, seqNumber.ToString("D4"));
        }
        #endregion
    }

}
