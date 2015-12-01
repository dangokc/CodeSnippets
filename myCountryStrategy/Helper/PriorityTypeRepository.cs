using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace CountryStrategy.Models.Helper
{
    public static class PriorityTypeRepository
    {
        public static IList<PriorityType> GetAllPriorityType(this AmarisEntities db, Logger log)
        {
            try
            {
                return db.PriorityTypes.ToList();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new List<PriorityType>();
            }
        }
        public static IList<PriorityType> GetAllPriorityType(this AmarisEntities db, Logger log, bool isDeleted)
        {
            try
            {
                return db.PriorityTypes.Where(x => x.IsDeleted == isDeleted).ToList();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new List<PriorityType>();
            }
        }
    }
}