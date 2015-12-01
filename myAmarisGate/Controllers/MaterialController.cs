using System.Linq;
using System.Net;
using System.Web.Mvc;
using Amaris.Statistics;
using AmarisGate.Dal;
using System;
using AmarisGate.Model;
using Amaris.Application.Attributes;

namespace AmarisGate.Controllers
{
    public class MaterialController : BootstrapBaseController
    {
        [HttpGet]
        [UsageCounter]
        public ActionResult Index()
        {
            var model = DB.GenericMaterials.OrderBy(x => x.GenericMaterialId).ToList();
            return View(model);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult Create(string materialName, string materialCode, bool requiresSetup, bool isDisabled, string materialDetails, int CategoryId)
        {
            try
            {
                /*We do not check if the material being created is repeated or not. Is there such a need?*/
                GenericMaterial newMaterial = new GenericMaterial
                {
                    Label = materialName,
                    MaterialCode = materialCode,
                    RequiresSetup = requiresSetup,
                    Disabled = isDisabled,
                    Details = materialDetails,
                    CategoryId = CategoryId
                };
                
                DB.GenericMaterials.Add(newMaterial);
                DB.SaveChanges();
                //TODO Remove all occurrences of this kind of code return => it is of any use to the user
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UsageCounter]
        public void Delete(int id)
        {
            var material = DB.GenericMaterials.Find(id);
            
            foreach (Package aPackage in material.Packages)
            {
                /* 
                 * If the size of materials in this package is 1, it means that this item is the only item it has. 
                 * Since we are going to delete this item, we might as well just delete the package so we dont end 
                 * up having packages with no materials.
                 */
                if (aPackage.GenericMaterials.Count == 1)
                {
                    aPackage.GenericMaterials.Clear();
                    aPackage.Functions.Clear();
                    aPackage.Companies.Clear();
                    DB.Packages.Remove(aPackage);
                }
                else
                {
                    aPackage.GenericMaterials.Remove(material);
                }
            }

            DB.GenericMaterials.Remove(material);
            DB.SaveChanges();
        }

        [HttpGet]
        public ActionResult DeleteMaterialDetails(int id)
        {
            var material = DB.GenericMaterials.Find(id); 
            var packages = material.Packages;

            var messageBody = "Are you sure you wish to delete this material?";
            if(packages.Count != 0)
            {
                messageBody += " Deleting this material will affect the following package(s): ";

                foreach(Package aPackage in packages)
                {
                   messageBody += string.Join(", ", aPackage.PackageName);
                }
            }

            ModalPopUp deleteDialog = new ModalPopUp("Deleting a Material", messageBody, Url.Action("Delete", "Material"), "Delete Material");
            return PartialView("_ModalDeleteConfirmation", deleteDialog);
        }

        public ActionResult GetCategoryList(string query = "", int id = 0)
        {
            var lstPt = DB.GATE_MaterialCategory  
                .Select(item => new
                {
                    id = item.CategoryId,
                    text = item.CategoryLabel
                })
                .ToList();

            return Json(lstPt, JsonRequestBehavior.AllowGet);
        }

        
        [HttpGet]
        public ActionResult CreateCategory()
        {
            return PartialView("CreateCategory");            
        }

        [HttpPost]
        public ActionResult CreateCategory(string categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName))
            {
                var query = DB.GATE_MaterialCategory.Any(x => x.CategoryLabel == categoryName);
                if (!query)
                {
                    GATE_MaterialCategory newCategory = new GATE_MaterialCategory
                    {
                        CategoryLabel = categoryName
                    };

                    DB.GATE_MaterialCategory.Add(newCategory);
                    DB.SaveChanges();

                }
                else
                {
                    return Json(new { status = "error", message = "Category is already existed" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = "error", message = "Category cannot be Null" }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { status = "success", message = "Category is added successfully" }, JsonRequestBehavior.AllowGet);
            
        }

        #region XEditables

        public ActionResult UpdateCategory(int pk, string name, string value)
        {
            if (name == null)
                name = "Material.Details";
            return XEditableUpdate(DB.GenericMaterials, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateDetails(int pk, string name, string value)
        {
            if (name == null)
                name = "Material.Details";
            return XEditableUpdate(DB.GenericMaterials, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateLabel(int pk, string name, string value)
        {
            if (name == null)
                name = "Material.Label";
            return XEditableUpdate(DB.GenericMaterials, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateCode(int pk, string name, string value)
        {
            if (name == null)
                name = "Material.MaterialCode";
            return XEditableUpdate(DB.GenericMaterials, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateLoanRequested(int pk, string name, string value)
        {
            if (name == null)
                name = "Material.LoanRequested";
            return XEditableUpdate(DB.GenericMaterials, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateRequired(int pk, string name, string value)
        {
            if (name == null)
                name = "Material.Required";

            foreach (GATE_ComponentRequest component in DB.GATE_ComponentRequest.Where(comp => comp.MaterialId == pk))
            {
                component.IsSetup = !Boolean.Parse(value);
            }
            
            return XEditableUpdate(DB.GenericMaterials, pk, name, value);
        }

        [HttpPost]
        [UsageCounter]
        public ActionResult UpdateDisabled(int pk, string name, string value)
        {
            if (name == null)
                name = "Material.Disabled";
            return XEditableUpdate(DB.GenericMaterials, pk, name, value);
        }

        #endregion XEditables
     
    }
}
