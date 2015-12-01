using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amaris.Application.Controllers;
using CountryStrategy.Models;

namespace CountryStrategy.Controllers
{
    public class AmarisController : AmarisBaseController<AmarisEntities, Employee>
    {
        /// <summary>
        /// The friendly name of the application, displayed in the page title.
        /// </summary>
        public override string ApplicationDescription
        {
            get { return ApplicationName; }
        }
    }
}
