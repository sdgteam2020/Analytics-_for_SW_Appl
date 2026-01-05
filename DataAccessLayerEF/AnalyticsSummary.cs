using Domain.interfaces;
using Domain.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class AnalyticsSummary : IAnalyticsSummary
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsSummary(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<DTOAnalyticsSummary> GetDataSummary(int ApplicationId, int UserId,int Months,int Years)
        {
            DTOAnalyticsSummary dTOAnalyticsSummary = new DTOAnalyticsSummary();
            var query = await (from trnapp in _context.trnapplicationDetails
                               join trnhitsum in _context.TrnMpplicationHitsSummary
                               on trnapp.ApplicationId equals trnhitsum.ApplicationId
                               where trnhitsum.Date.Month == Months && trnhitsum.Date.Year==Years
                                &&
                                (
                                    // Admin → All applications
                                    (ApplicationId == 0 && UserId == 0)

                                    // Admin → Specific application
                                    || (ApplicationId > 0 && UserId == 0
                                        && trnhitsum.ApplicationId == ApplicationId)

                                    // User → Own application only
                                    || (ApplicationId == 0 && UserId > 0 && trnapp.CreatedBy == UserId)
                                    || (ApplicationId > 0 && UserId > 0 && trnapp.ApplicationId == ApplicationId && trnapp.CreatedBy == UserId)
                                )
                               group trnhitsum by new
                               {
                                   trnapp.ApplicationName,
                                   Month = trnhitsum.Date.Month,  // Extract month
                                   Year = trnhitsum.Date.Year,    // Extract year
                                   trnhitsum.ApplicationId
                               } into g
                               select new DTOApplicationMonthsWise
                               {
                                   ApplicationName = g.Key.ApplicationName,
                                   MonthsName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                                   Years = g.Key.Year,
                                   TotalHits = g.Sum(x => x.TodayHits),

                               }).ToListAsync();


            dTOAnalyticsSummary.DTOApplicationMonthsWiseList = query;


            var quertTotalHits = await (from trnapp in _context.trnapplicationDetails
                                        join trnhitsum in _context.trnmApplicationHits
                                        on trnapp.ApplicationId equals trnhitsum.ApplicationId
                                        where  //trnhitsum.CreatedOn.Value.Month == Months && trnhitsum.CreatedOn.Value.Year == Years &&
                                (
                                    // Admin → All applications
                                    (ApplicationId == 0 && UserId == 0)

                                    // Admin → Specific application
                                    || (ApplicationId > 0 && UserId == 0
                                        && trnhitsum.ApplicationId == ApplicationId)

                                    // User → Own application only
                                    || (ApplicationId == 0 && UserId > 0 && trnapp.CreatedBy == UserId)
                                    || (ApplicationId > 0 && UserId > 0 && trnapp.ApplicationId == ApplicationId && trnapp.CreatedBy == UserId)
                                )
                                        select new DTOApplicationTotalCount
                                        {
                                            ApplicationName = trnapp.ApplicationName,
                                            TotalHits = trnhitsum.TotalHits,
                                            ColorCode = trnapp.ColorCode
                                        }).ToListAsync();
            dTOAnalyticsSummary.DTOApplicationTotalCountList = quertTotalHits;




            var daywise = await (from trnapp in _context.trnapplicationDetails
                                 join hitsum in _context.TrnMpplicationHitsSummary on trnapp.ApplicationId equals hitsum.ApplicationId
                                 where 
                                   (hitsum.Date.Year ==Years && hitsum.Date.Month == Months)
                                   &&
                                (
                                    // Admin → All applications
                                    (ApplicationId == 0 && UserId == 0)

                                    // Admin → Specific application
                                    || (ApplicationId > 0 && UserId == 0
                                        && trnapp.ApplicationId == ApplicationId)

                                    // User → Own application only
                                    || (ApplicationId == 0 && UserId > 0 && trnapp.CreatedBy == UserId)
                                    || (ApplicationId > 0 && UserId > 0 && trnapp.ApplicationId == ApplicationId && trnapp.CreatedBy == UserId)
                                )
                                 group hitsum by new
                                 {
                                     trnapp.ApplicationName,
                                     Month = hitsum.Date.Month,  // Extract month
                                     Day = hitsum.Date.Day,    // Extract year
                                     trnapp.ApplicationId
                                 } into g
                                 select new DTOApplicationDayWise
                                 {
                                     ApplicationName = g.Key.ApplicationName,
                                     MonthsName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                                     Month = g.Key.Month,
                                     Days = g.Key.Day,
                                     TotalHits = g.Sum(x => x.TodayHits),
                                 }).ToListAsync();

            dTOAnalyticsSummary.DTOApplicationDayWiseList = daywise;

            return dTOAnalyticsSummary;
        }

        public async Task<List<DTOApplicationDayWise>> GetMonthWise(int ApplicationId, int Month, int Year)
        {

            var daywise = await (from trnapp in _context.trnapplicationDetails
                                 join hitsum in _context.TrnMpplicationHitsSummary on trnapp.ApplicationId equals hitsum.ApplicationId
                                 where (ApplicationId == 0 || hitsum.ApplicationId == ApplicationId)
                                  && (hitsum.Date.Year == Year && hitsum.Date.Month == Month)
                                 group hitsum by new
                                 {
                                     trnapp.ApplicationName,
                                     Month = hitsum.Date.Month,  // Extract month
                                     Day = hitsum.Date.Day,    // Extract year
                                     trnapp.ApplicationId
                                 } into g
                                 select new DTOApplicationDayWise
                                 {
                                     ApplicationName = g.Key.ApplicationName,
                                     MonthsName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                                     Month = g.Key.Month,
                                     Days = g.Key.Day,
                                     TotalHits = g.Sum(x => x.TodayHits),
                                 }).ToListAsync();

            return daywise;
        }

        public async Task<List<DTOApplicationMonthsWise>> GetYearWise(int ApplicationId, int Years)
        {
            List<DTOApplicationMonthsWise> dTOAnalyticsSummary = new List<DTOApplicationMonthsWise>();
            var query = await (from trnapp in _context.trnapplicationDetails
                               join trnhitsum in _context.TrnMpplicationHitsSummary
                               on trnapp.ApplicationId equals trnhitsum.ApplicationId
                               where (ApplicationId == 0 || trnhitsum.ApplicationId == ApplicationId)
                               && trnhitsum.Date.Year == Years
                               group trnhitsum by new
                               {
                                   trnapp.ApplicationName,
                                   Month = trnhitsum.Date.Month,  // Extract month
                                   Year = trnhitsum.Date.Year,    // Extract year
                                   trnhitsum.ApplicationId
                               } into g
                               select new DTOApplicationMonthsWise
                               {
                                   ApplicationName = g.Key.ApplicationName,
                                   MonthsName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                                   Years = g.Key.Year,
                                   TotalHits = g.Sum(x => x.TodayHits),

                               }).ToListAsync();




            return query;
        }
    }
}
