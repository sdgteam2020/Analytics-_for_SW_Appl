using Domain.CommonModel;
using Domain.interfaces;
using Domain.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class CentralAnalyticsService : ICentralAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public CentralAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Increment the hit counters for a specific application identified by AppKey
        public async Task IncrementHitCounter(int ApplicationId)
        {
            var today = DateTime.Today;

            // Get or create application entry in the database
            var appHits = await _context.trnmApplicationHits
                .Where(a => a.ApplicationId == ApplicationId)
                .FirstOrDefaultAsync();

            if (appHits == null)
            {
                appHits = new MApplicationHits
                {
                    ApplicationId = ApplicationId,
                    //Date = today,
                    TotalHits = 1
                    //TodayHits = 1,
                    //MonthHits = 1
                };
                _context.trnmApplicationHits.Add(appHits);
            }
            else
            {
                appHits.TotalHits++;
                //appHits.TodayHits++;
                //appHits.MonthHits++;
            }

            await _context.SaveChangesAsync();
        }

        // Fetch data for a specific AppKey
        public async Task<MApplicationHits> GetApplicationHits(int ApplicationId)
        {
            var today = DateTime.Today;
            return await _context.trnmApplicationHits
                .Where(a => a.ApplicationId == ApplicationId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalHitsForApp(int ApplicationId)
        {
            var appHits = await _context.trnmApplicationHits
                .Where(a => a.ApplicationId == ApplicationId)
                .SumAsync(a => a.TotalHits);

            return appHits;
        }

        public async Task<int> GetTodayHitsForApp(int ApplicationId)
        {
            var appHits = await _context.trnmApplicationHits
                .Where(a => a.ApplicationId == ApplicationId)
                .SumAsync(a => a.TotalHits);

            return appHits;
        }

        public async Task<int> GetMonthHitsForApp(int ApplicationId)
        {
            var currentMonth = DateTime.Now.Month;
            var appHits = await _context.trnmApplicationHits
                .Where(a => a.ApplicationId == ApplicationId)
                .SumAsync(a => a.TotalHits);

            return appHits;
        }

        //public async Task<DTOCounterHitsResponse> HitCounter(int ApplicationId)
        //{
        //    DTOCounterHitsResponse dTOCounterHitsResponse = new DTOCounterHitsResponse();
        //    var currentDate = DateTime.Today;
        //    var currentMonth = DateTime.Now.Month;
        //    var currentYear = DateTime.Now.Year;

        //    var hitCounts = await _context.trnmApplicationHits
        //        .Where(a => a.ApplicationId == ApplicationId)
        //        .GroupBy(a => 1)  // Grouping by a constant because we only want one aggregate result
        //        .Select(g => new
        //        {
        //            TotalHits = g.Sum(a => a.TotalHits),
        //            TodayHits = g.Where(a => a.Date.Date == currentDate).Sum(a => a.TotalHits),
        //            MonthlyHits = g.Where(a => a.Date.Month == currentMonth && a.Date.Year == currentYear).Sum(a => a.TotalHits)
        //        })
        //        .FirstOrDefaultAsync();  // Use FirstOrDefaultAsync to get the first result from the aggregation

        //    // After this, you can access the values like:

        //    dTOCounterHitsResponse.TotalHits = hitCounts?.TotalHits ?? 0;
        //    dTOCounterHitsResponse.TodayHits = hitCounts?.TodayHits ?? 0;
        //    dTOCounterHitsResponse.MonthlyHits = hitCounts?.MonthlyHits ?? 0;
        //    return dTOCounterHitsResponse;
        //}
        public async Task<DTOCounterHitsResponse> HitCounter(int ApplicationId)
        {

            var currentDate = DateTime.Today;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var hitCounts = await (
      from a in _context.TrnMpplicationHitsSummary
      join b in _context.trnmApplicationHits on a.ApplicationId equals b.ApplicationId
      where a.ApplicationId == ApplicationId && a.Date.Month == currentMonth
      select new DTOCounterHitsResponse
      {
          TotalHits = b.TotalHits,
         // TodayHits = a.TodayHits,
          //MonthlyHits = a.MonthHits // was MonthHits?
      }

      ).AsNoTracking().FirstOrDefaultAsync();
            var TodayHits = await (
                from a in _context.TrnMpplicationHitsSummary
                join b in _context.trnmApplicationHits on a.ApplicationId equals b.ApplicationId
                where a.ApplicationId == ApplicationId && a.Date.Date == currentDate.Date
                select new DTOCounterHitsResponse
                {
                   
                    TodayHits = a.TodayHits,
                    //MonthlyHits = a.MonthHits // was MonthHits?
                }

                ).AsNoTracking().FirstOrDefaultAsync();

            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);  // First day of the current month
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);  // Last day of the current month

            var totalHits = await _context.TrnMpplicationHitsSummary
            .Where(a => a.ApplicationId == ApplicationId &&
                    a.Date >= startOfMonth &&  // Ensure the date is after the start of the current month
                    a.Date <= endOfMonth)  // Ensure the date is before the end of the current month
            .SumAsync(a => a.TodayHits);  // Sum of TotalHits for the date range


            if (hitCounts != null)
            {
                hitCounts.MonthlyHits = totalHits;
                hitCounts.TodayHits = TodayHits.TodayHits;

            }
            return hitCounts;
        }

        public async Task<List<DTOHitForDgisResponse>> HitCounter()
        {
            var today = DateTime.Today;
            var startOfToday = today;
            var endOfToday = today.AddDays(1);

            var firstOfMonth = new DateTime(today.Year, today.Month, 1);
            var nextMonth = firstOfMonth.AddMonths(1);

            // Query to get active users and store in a dictionary
            var activeUsers = await (
                 from app in _context.trnapplicationDetails
                 join sess in _context.trnapplicationSessions
                     on app.ApplicationId equals sess.ApplicationId
                 where sess.SessionEndTime == null
                       && EF.Functions.DateDiffMinute(sess.LastUpdated, DateTime.Now) <= 4
                       && sess.IsActive == true
                 group sess by app.ApplicationId into g
                 select new
                 {
                     ApplicationId = g.Key,
                     ActiveUserCount = g.Count()
                 }
             ).ToDictionaryAsync(u => u.ApplicationId, u => u.ActiveUserCount);


            var result =
              await _context.trnapplicationDetails.AsNoTracking()
                .Select(app => new DTOHitForDgisResponse
                {

                    Project = app.ApplicationName,

                    // from TrnMpplicationHitsSummary (aggregated)
                    Today = _context.TrnMpplicationHitsSummary
                                .Where(hs => hs.ApplicationId == app.ApplicationId
                                          && hs.Date >= startOfToday && hs.Date < endOfToday)
                                .Sum(hs => (int?)hs.TodayHits) ?? 0,

                    CurrentMonth = _context.TrnMpplicationHitsSummary
                                .Where(hs => hs.ApplicationId == app.ApplicationId
                                          && hs.Date >= firstOfMonth && hs.Date < nextMonth)
                                .Sum(hs => (int?)hs.TodayHits) ?? 0,

                    // from trnmApplicationHits (single row per app; pick the value if present)
                    Total = _context.trnmApplicationHits
                                .Where(ah => ah.ApplicationId == app.ApplicationId)
                                .Select(ah => (int?)ah.TotalHits)
                                .FirstOrDefault() ?? 0,
                    Concurrentuser = activeUsers.ContainsKey(app.ApplicationId)
                        ? activeUsers[app.ApplicationId]
                        : 0

                })
                .OrderBy(x => x.Project)
                .ToListAsync();

            return result;
        }
    }
}
