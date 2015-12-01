using Amaris.Statistics;
using Amaris.XmlExport;
using AmarisGate.Dal;
using AmarisGate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AmarisGate.Controllers
{
    public class ExcelExportController : BootstrapBaseController
    {

        public const string SCHEMA_NAME = "AmarisGate.xsd";

        [HttpGet]
        [UsageCounter]
        public ActionResult GetExport()
        {
            //Return data
            return new XmlActionResult<object>(GetList(), SCHEMA_NAME);
        }

        [HttpGet]
        [UsageCounter]
        public ActionResult GetSchema()
        {
            //Generate schema
            return new XsdActionResult(GetList(), SCHEMA_NAME);
        }

        private List<XMLPackageEntry> GetList()
        {
            return new XMLPackages(DB.Packages).entries;
        }
    }
}