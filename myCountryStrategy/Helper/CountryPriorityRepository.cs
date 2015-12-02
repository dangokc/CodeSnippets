using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NLog;

namespace CountryStrategy.Models.Helper
{
    public static class CountryPriorityRepository
    {
        /// <summary>
        /// Desc: Get all with condition or no condition
        /// </summary>
        #region GetAllPriority()
        public static IEnumerable<Priority> GetAllPriority(this AmarisEntities db, Logger log)
        {
            try
            {
                return db.Priorities;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public static IEnumerable<Priority> GetAllPriority(this DbSet<Priority> priorities, Logger log, bool isDeleted)
        {
            try
            {
                return priorities
                    .Where(x => x.IsDeleted == isDeleted);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }
        #endregion

        public static IEnumerable<Priority> GetAllCountry(this AmarisEntities db, Logger log)
        {
            try
            {
                return db.Priorities;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        #region GetPriorityWithFilter()
        /// <summary>
        /// Desc: Search by keyword & Filter
        /// </summary>
        public static IEnumerable<Priority> GetPriorityWithFilter(this AmarisEntities db, Logger log, string keyword, List<long> lstPriorityTypeId, List<long> lstDeadlineTypeId)
        {
            try
            {
                //Desc: Solve text issue & Search by keyword
                var strSearchParameter = keyword.Trim().Replace("*", "").Replace("@", "").ToLower();
                // Apply search by TEXT
                IEnumerable<Priority> lstOpeningCountryPriorities = db.Priorities
                    .Where(x => x.Country.Label.ToLower().Contains(strSearchParameter)).ToList();
                
                // Check if filter PriorityType have data 
                if (lstPriorityTypeId.Any())
                {
                    lstOpeningCountryPriorities =
                        lstOpeningCountryPriorities.Where(x => lstPriorityTypeId.Any(prio => x.PriorityTypeId == prio));
                }
                // Check if filter Deadline Type have data
                if (lstDeadlineTypeId.Any())
                {
                    lstOpeningCountryPriorities =
                        lstOpeningCountryPriorities.Where(x => lstDeadlineTypeId.Any(deadLine => x.DeadlineTypeId == deadLine));
                }

                return lstOpeningCountryPriorities;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }
        #endregion
    
        /// <summary>
        /// Desc: Order by priority (from high to low)
        /// </summary>
        public static IEnumerable<Priority> DefaultOrder(this IEnumerable<Priority> countryPriorities)
        {
            return countryPriorities.OrderBy(x => x.PriorityOrder);
        }
        public static IEnumerable<Priority> OrderByPriority(this IEnumerable<Priority> countryPriorities)
        {
            return countryPriorities.OrderBy(x => x.PriorityType.LevelRole);
        }
        public static IEnumerable<Priority> OrderByPriorityOrder(this IEnumerable<Priority> countryPriorities)
        {
            return countryPriorities.OrderBy(x => x.PriorityOrder);
        }
        public static IEnumerable<Priority> OrderByCountry(this IEnumerable<Priority> countries)
        {
            return countries.OrderBy(x => x.Country.Label);            
        }
        public static IEnumerable<Priority> OrderByDeadline(this IEnumerable<Priority> countryPriorities)
        {
            return countryPriorities.OrderBy(x => x.DeadlineDate);
        }

        public static void UpdatePriorityOrder(this AmarisEntities db, int id = 0, int order = 0)
        {
            db.UpdatePriorityOrder(id, order);
        }

        public static void UpdateAFMPriority(this AmarisEntities db, int pk, List<int> value)
        {
            var priority = db.Priorities.Find(pk);
            priority.AfmEmployeeGroup.Clear();

            if (value != null && value.Any())
            {
                var employees = db.Employee.Where(p => value.Contains(p.EmployeeId)).ToList();
                employees.ForEach(p => priority.AfmEmployeeGroup.Add(p));
            }
            db.SaveChanges();
        }

        public static void UpdateDirectorPriority(this AmarisEntities db, int pk, List<int> value)
        {
            var priority = db.Priorities.Find(pk);
            priority.DirectorEmployeeGroup.Clear();

            if (value != null && value.Any())
            {
                var employees = db.Employee.Where(p => value.Contains(p.EmployeeId)).ToList();
                employees.ForEach(p => priority.DirectorEmployeeGroup.Add(p));
            }           
            db.SaveChanges();
        }

        public static void UpdateCorpDevPriority(this AmarisEntities db, int pk, List<int> value)
        {
            var priority = db.Priorities.Find(pk);
            priority.CorpDevEmployeeGroup.Clear();

            if (value != null && value.Any())
            {
                var employees = db.Employee.Where(p => value.Contains(p.EmployeeId)).ToList();
                employees.ForEach(p => priority.CorpDevEmployeeGroup.Add(p));
            }
            db.SaveChanges();            
        }
    }
}
