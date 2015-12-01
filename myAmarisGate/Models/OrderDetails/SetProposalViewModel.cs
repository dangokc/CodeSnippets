using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AmarisGate.Dal;

namespace AmarisGate.Model.OrderDetails
{
    public class SetProposalViewModel
    {
        public int ComponentId { get; set; }

        public int MaterialRequestId { get; set; }

        public int EmployeeId { get; set; }

        public DateTime ExitDate { get; set; }
    }
}