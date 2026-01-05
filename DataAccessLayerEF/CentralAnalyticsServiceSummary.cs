using Domain.CommonModel;
using Domain.interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class CentralAnalyticsServiceSummary : ICentralAnalyticsServiceSummary
    {
        private readonly ApplicationDbContext _context;

        public CentralAnalyticsServiceSummary(ApplicationDbContext context)
        {
            _context = context;
        }
        // Increment the hit counters for a specific application identified by AppKey
        public async Task IncrementHitCounter(int ApplicationId)
        {
            var today = DateTime.Today;
            // Get or create application entry in the database
            var appHits = await _context.TrnMpplicationHitsSummary
                .Where(a => a.ApplicationId == ApplicationId && a.Date == today)
                .FirstOrDefaultAsync();

            if (appHits == null)
            {

                appHits = new MApplicationHitsSummary
                {
                    ApplicationId = ApplicationId,
                    Date = today,
                    TodayHits = 1,
                    // MonthHits = 1
                };
                _context.TrnMpplicationHitsSummary.Add(appHits);
            }
            else
            {

                appHits.TodayHits++;
                // appHits.MonthHits++;
            }

            await _context.SaveChangesAsync();
        }
    }
}
