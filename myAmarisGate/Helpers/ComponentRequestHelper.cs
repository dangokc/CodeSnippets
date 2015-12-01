using System;
using AmarisGate.Dal;

namespace AmarisGate.Helpers
{
    public static class ComponentRequestHelper
    {
        public static Boolean IsEditable(this GATE_ComponentRequest comp, Boolean isHelpDesk)
        {
            return isHelpDesk && comp.StatusId == OrderStatus.HelpDeskProductProposal;
        }
    }
}