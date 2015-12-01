using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Amaris.Statistics;
using AmarisGate.Dal;
using AmarisGate.Model;
using Developpez.Dotnet.Collections;

namespace AmarisGate.Controllers
{
    public class PackageController : BootstrapBaseController
    {
        [HttpGet]
        [UsageCounter]
        public ActionResult Index()
        {
            IEnumerable<Package> model = DB.Packages.Include(x => x.Companies)
                                                    .Include(x => x.GenericMaterials)
                                                    .Include(x => x.Functions)
                                                    .OrderBy(x => x.PackageId);
            return View(model);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult CreatePackage(string newPkgName, string newFunctions, string newMaterials, string newCompanies)
        {
            try
            {
                Package newPkg = new Package {PackageName = newPkgName};
                StringToCollections(newPkg, newMaterials, newFunctions, newCompanies);

                //check for repeated functions in packages
                List<string> repeatedPackageNames = new List<string>();
                List<string> repeatedFunctionNames = new List<string>();
                IEnumerable<Employee_Function> tmpFunctionsInt;
                foreach (Package aPackage in DB.Packages)
                {
                    tmpFunctionsInt = aPackage.Functions.Intersect(newPkg.Functions);
                    if (tmpFunctionsInt.Any())
                    {
                        repeatedPackageNames.Add(aPackage.PackageName);
                        foreach (Employee_Function aFunction in tmpFunctionsInt)
                        {
                            if (!repeatedFunctionNames.Contains(aFunction.Label))
                            {
                                repeatedFunctionNames.Add(aFunction.Label);
                            }
                        }
                    }
                }

                if (!repeatedPackageNames.IsNullOrEmpty())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Conflict, "Packages " + String.Join(", ", repeatedPackageNames)
                        + " already have the following functions: " + String.Join(", ", repeatedFunctionNames));
                }


                newPkg.CreatedDate = DateTime.Now;
                DB.Packages.Add(newPkg);
                DB.SaveChanges();

                Dictionary<string, string> rowInfo = new Dictionary<string, string>();
                rowInfo.Add("id", newPkg.PackageId.ToString());
                rowInfo.Add("pkgName", newPkg.PackageName);
                rowInfo.Add("materials", String.Join(" | ", newPkg.GenericMaterials.ToDictionary(mat => mat.GenericMaterialId, mat => mat.Label).Values));
                rowInfo.Add("functions", String.Join(" | ", newPkg.Functions.ToDictionary(fun => fun.FunctionId, fun => fun.Label).Values));
                rowInfo.Add("companies", newPkg.Companies.Any() ? String.Join(" | ", newPkg.Companies.ToDictionary(com => com.ID, com => com.CompanyCodeName).Values) : "All Companies");
                rowInfo.Add("deleteLink", Url.Action("Delete", "Package"));
                rowInfo.Add("editLink", Url.Action("GetPackageDetails", "Package"));

                return Json(rowInfo);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult EditPackage(int id, string newPkgName, string newFunctions, string newMaterials, string newCompanies)
        {
            try
            {
                //create test package to check for conflicts
                Package newPkg = new Package();
                StringToCollections(newPkg, newMaterials, newFunctions, newCompanies);
                newPkg.PackageId = id;
                if (!newPkgName.IsNullOrEmpty())
                {
                    newPkg.PackageName = newPkgName;
                }

                //check for repeated functions in packages
                List<string> repeatedPackageNames = new List<string>();
                List<string> repeatedFunctionNames = new List<string>();
                List<Employee_Function> tmpFunctionsInt;
                foreach (Package aPackage in DB.Packages)
                {
                    tmpFunctionsInt = aPackage.Functions.Intersect(newPkg.Functions).ToList();
                    if (tmpFunctionsInt.Any() && (aPackage.PackageId != newPkg.PackageId))
                    {
                        repeatedPackageNames.Add(aPackage.PackageName);
                        foreach (Employee_Function aFunction in tmpFunctionsInt)
                        {
                            if (!repeatedFunctionNames.Contains(aFunction.Label))
                            {
                                repeatedFunctionNames.Add(aFunction.Label);
                            }
                        }
                    }
                }

                if (!repeatedPackageNames.IsNullOrEmpty())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Conflict, "Packages " + String.Join(", ", repeatedPackageNames)
                        + " already have the following functions: " + String.Join(", ", repeatedFunctionNames));
                }

                //otherwise, update old package and save
                Package oldPkg = DB.Packages.Find(id);
                oldPkg.Functions.Clear();
                oldPkg.GenericMaterials.Clear();
                oldPkg.Companies.Clear();
                oldPkg.GenericMaterials = newPkg.GenericMaterials;
                oldPkg.Functions = newPkg.Functions;
                oldPkg.Companies = newPkg.Companies;
                if (!newPkgName.IsNullOrEmpty())
                {
                    oldPkg.PackageName = newPkgName;
                }
                oldPkg.UpdatedDate = DateTime.Now;
                DB.SaveChanges();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public void Delete(int id)
        {
            //Delete the association with this package  
            IQueryable<GATE_MaterialRequest> linkedRequests =
                from aRequest in DB.GATE_MaterialRequest
                where aRequest.PackageId == id
                select aRequest;
            foreach (GATE_MaterialRequest req in linkedRequests)
            {
                req.PackageId = null;
            }

            Package package = DB.Packages.Find(id);
            package.GenericMaterials.Clear();
            package.Functions.Clear();
            package.Companies.Clear();
            DB.Packages.Remove(package);
            DB.SaveChanges();
        }

        [HttpGet]
        public ActionResult GetPackageDetails(int id)
        {
            Package pkg = DB.Packages.Find(id);

            IEnumerable<int> materialIds = pkg.GenericMaterials.Select(mat => mat.GenericMaterialId);
            IEnumerable<int> functionIds = pkg.Functions.Select(fun => fun.FunctionId);
            IEnumerable<int> companyIds = pkg.Companies.Select(comp => comp.ID);

            string materialsString = string.Join(",", materialIds);
            string functionsString = string.Join(",", functionIds);
            string companiesString = string.Join(",", companyIds);

            return PartialView("_ModalPackagePopup", new ModalPackageInfo("packageModalPopup", pkg.PackageId, pkg.PackageName, materialsString, functionsString, companiesString, Url.Action("EditPackage", "Package"), false));
        }

        #region AuxiliaryMethods

        private void StringToCollections(Package inPackage, string materialIdsStr, string functionIdsStr, string companyIdsStr)
        {
            if (!materialIdsStr.IsNullOrEmpty())
            {
                int[] materialIds = Array.ConvertAll<string, int>(materialIdsStr.Split(','), int.Parse);
                inPackage.GenericMaterials = DB.GenericMaterials.Where(mat => materialIds.Contains(mat.GenericMaterialId)).ToList();
            }
            
            if(!functionIdsStr.IsNullOrEmpty()){
                int[] fucntionIds = Array.ConvertAll<string, int>(functionIdsStr.Split(','), int.Parse);
                inPackage.Functions = DB.Employee_Function.Where(x => fucntionIds.Contains(x.FunctionId)).ToList();
            }

            if (!companyIdsStr.IsNullOrEmpty())
            {
                int[] companyIds = Array.ConvertAll<string, int>(companyIdsStr.Split(','), int.Parse);
                inPackage.Companies = DB.Companies.Where(x => companyIds.Contains(x.ID)).ToList();
            }
        }

        #endregion AuxiliaryMethods
    }
}