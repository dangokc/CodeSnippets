using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AmarisGate.Dal;

namespace AmarisGate.Model.OrderDetails
{
    public class ComponentRowModel
    {
        /*public ComponentRowModel(GATE_ComponentRequest comp, OrderDetailsModel model)
        {
            Order = model;
            Component = comp;
            HasCustomAddress = model.Address != null && !model.Address.Complement.Equals(string.Empty);
            HasAmarisOffice = model.AmarisOfficeId != null && model.AmarisOffice != null;
        }*/

        public GATE_MaterialSuggestion CurrentSuggestion { get; set; }
        public GenericMaterial Material { get; set; }
        public OrderDetailsModel Order { get; set; }
        public GATE_ComponentRequest Component { get; set; }
        public Boolean HasCustomAddress { get; set; }
        public Boolean HasAmarisOffice { get; set; }
        public string StatusLabel { get; set; }

        public ProductPassword ScreenCode { get; set; }

        public int? PreviousUserId { get; set; }

        public DateTime? ExitDate { get; set; }

        public string PreviousUser { get; set; }

        public Boolean IsFromStock { get; set; }
    
    }
}