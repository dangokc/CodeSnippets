using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AmarisGate.Dal;
using AmarisGate.Model.OrderDetails;

namespace AmarisGate.Model
{
    public class OrderDetailsModel
    {
        private ContactInfo _concernedEmpInfo;

        public ContactInfo ConcernedEmpInfo
        {
            get
            {
                if (_concernedEmpInfo == null)
                    _concernedEmpInfo = new ContactInfo(ConcernedEmployee, ConcernedExtNumber);
                return _concernedEmpInfo;
            }
            set { _concernedEmpInfo = value; }
        }
        private ContactInfo _LOMInfo;

        public ContactInfo LOMInfo
        {
            get
            {
                if (_LOMInfo == null && LOMEmployees.Any())
                    _LOMInfo = new ContactInfo(LOMEmployees.FirstOrDefault(), LOMExtNumber);
                return _LOMInfo;
            }
            set { _LOMInfo = value; }
        }
        private ContactInfo _orderedByEmpInfo;

        public ContactInfo OrderedByEmpInfo
        {
            get
            {
                if (_orderedByEmpInfo == null)
                    _orderedByEmpInfo = new ContactInfo(OrderedEmployee, OrderedExtNumber);
                return _orderedByEmpInfo;
            }
            set { _orderedByEmpInfo = value; }
        }
        public Employee ConcernedEmployee { get; set; }

        public Employee EntryDate { get; set; }
        public Employee OrderedEmployee { get; set; }
        public IEnumerable<Employee> LOMEmployees { get; set; }
        public IEnumerable<Employee> CompanyDirectors { get; set; }
        //public IEnumerable<Employee> LocalOfficeManagers { get; set; }
        public int? AmarisOfficeId { get; set; }
        public Office AmarisOffice { get; set; }

        public string AmarisOfficeName { get; set; }
        public DateTime RequestDate { get; set; }

        

       
        public int OrderId { get; set; }
        public Package Package { get; set; }

        public int? PackageId
        {
            get { return Package == null ? (int?)null : Package.PackageId; }
        }
        public double? TotalCost { get; set; }
        public IEnumerable<ComponentRowModel> Components { get; set; }
        public string CurrencyId { get; set; }
        public string Comment { get; set; }
        public Address Address { get; set; }
        public string LOMExtNumber { get; set; }
        public string ConcernedExtNumber { get; set; }
        public string OrderedExtNumber { get; set; }
        public OrderStatus BiggestStatus { get; set; }
        public int? BiggestStatusId { get; set; }

        public string DNALink
        {
            get { return "https://arp.amaris.com/DNA/Employee/EmployeeList/Details/" + ConcernedEmployee.EmployeeId; }
        }

        public string AShieldLink
        {
            get { return "/aShield/Monitor/Details/" +  ConcernedEmployee.EmployeeId; }

        }
        public bool HasAmarisOffice
        {
            get { return AmarisOfficeId != null && AmarisOffice != null; }
        }

        public bool HasCustomAddress
        {
            get { return (Address != null && !Address.Complement.Equals(string.Empty)); }
        }

        //public bool HasCustomMaterial
        //{
        //    get { return CustomMaterialsList.Any(); }
        //}

        public bool HasExitDate
        {
            get { return ConcernedEmployee.ExitDate != null; }
        }


        public bool HasPackage
        {
            get { return Package != null && Components.Any(); }
        }
        public static Expression<Func<GATE_MaterialRequest, OrderDetailsModel>> ContructorForEF;
        static OrderDetailsModel()
        {
            ContructorForEF = order => new OrderDetailsModel
            {
                OrderId = order.MaterialRequestId,
                ConcernedEmployee = order.ConcernedEmployee,
                AmarisOfficeId = order.AmarisOfficeId,
                AmarisOffice = order.Office,
                AmarisOfficeName = order.Office == null ? "" : order.Office.Name,
                RequestDate = order.RequestDate,
                Package = order.Package,
                TotalCost = order.ComponentRequests.Sum(y => y.CurrentSuggestion.Price),
                Components = order.ComponentRequests.Select(comp => new ComponentRowModel
                {
                    Component = comp,
                    HasCustomAddress = order.Address != null && !order.Address.Complement.Equals(string.Empty),
                    HasAmarisOffice = order.AmarisOfficeId != null && order.Office != null,
                    CurrentSuggestion = comp.CurrentSuggestion,
                    Material = comp.GenericMaterial,
                    StatusLabel = comp.Status.Label,

                    PreviousUser = comp.Gate_StockHistory.User.Lastname + " " + comp.Gate_StockHistory.User.Firstname,
                    ExitDate = comp.Gate_StockHistory.User.ExitDate,
                    IsFromStock = order.ConcernedEmployee.EmployeeId == comp.Gate_StockHistory.UserId,

                }),
                CurrencyId = (order.Currency == null ? "EUR" : order.CurrencyId),
                Comment = order.Comment,
                Address = order.Address,
                LOMEmployees = order.Office.LocalOfficeManager,
                OrderedEmployee = order.OrderedByEmployee,
                BiggestStatusId = order.ComponentRequests.Any() ? order.ComponentRequests.Max(component => (int)component.StatusId) : (int?)null,
                LOMExtNumber = order.AmarisOfficeId != null && order.Office != null && order.Office.LocalOfficeManager.Any() ? order.Office.LocalOfficeManager.FirstOrDefault().Telecoms.Where(num => num.PhoneTypeId == 1).Select(x => x.PhoneNumber).FirstOrDefault() : null,
                ConcernedExtNumber = order.ConcernedEmployee.Telecoms.Where(num => num.PhoneTypeId == 1).Select(x => x.PhoneNumber).FirstOrDefault(),
                OrderedExtNumber = order.OrderedByEmployee.Telecoms.Where(num => num.PhoneTypeId == 1).Select(x => x.PhoneNumber).FirstOrDefault(),
                CompanyDirectors = order.ConcernedEmployee.WorkingForCompany.CompanyDirectors
            };
        }
        public Boolean hasLOMInfo()
        {
            return LOMInfo != null;
        }

        public bool CanXEdit(bool isHelpDesk)
        {
            return BiggestStatusId != null && (isHelpDesk && (OrderStatus)BiggestStatusId == OrderStatus.HelpDeskProductProposal);
        }
    }
}