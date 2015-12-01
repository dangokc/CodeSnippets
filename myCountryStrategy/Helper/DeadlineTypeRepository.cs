using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace CountryStrategy.Models.Helper
{
    public class DeadlineTypeRepository
    {
        public IList<DeadlineType> GetAllDeadlineType(AmarisEntities db, Logger log, bool isDeleted)
        {
            try
            {
                return db.DeadlineTypes.Where(x => x.IsDeleted == isDeleted).ToList();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new List<DeadlineType>();
            }
        }
    }
}