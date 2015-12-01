using AmarisGate.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmarisGate.Helpers
{
    public class StockHelper
    {
        private Entities _dbContext;
        private Employee _loggedEmployee;

        public StockHelper(Entities Db, Employee loggedEmployee)
        {
            this._dbContext = Db;
            this._loggedEmployee = loggedEmployee;
        }
        public string FindCompanyCodeNameFromEmployee(int employeeId)
        {
            var employeeByWorkingForId = _dbContext.Employees
                                            .Where(x => x.EmployeeId == employeeId)
                                            .Select(x => x.WorkingForId)
                                            .FirstOrDefault();

            var companyCodeName = _dbContext.Companies
                                                .Where(x => x.ID == employeeByWorkingForId)
                                                .Select(x => x.CompanyCodeName)
                                                .FirstOrDefault();

            return companyCodeName;
        }

        public string FindMaterialCodeFromComponentId(int componenetId)
        {
            var materialId = _dbContext.GATE_ComponentRequest
                                        .Where(x => x.ComponentId == componenetId)
                                        .Select(x => x.MaterialId)
                                        .FirstOrDefault();

            var materialCode = _dbContext.GenericMaterials
                                .Where(x => x.GenericMaterialId == materialId)
                                .Select(x => x.MaterialCode)
                                .FirstOrDefault();

            return materialCode;
        }
        public int GetNextSeqNumber(int companyId, int materialId)
        {
            var list = _dbContext.Stocks
                                 .Where(x => x.CompanyId == companyId && x.GenericMaterialId == materialId);

            if (list.Any())
                return list.Max(x => x.SeqNumber) + 1;

            return 0;
        }
        public string GenerateProductCode(string companyCodeName, string materialLabel, int seqNumber)
        {
            return string.Format("{0}_{1}_{2}", companyCodeName, materialLabel, seqNumber.ToString("D4"));
        }
    }
}