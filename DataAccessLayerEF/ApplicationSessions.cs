using Domain.CommonModel;
using Domain.interfaces;
using Domain.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class ApplicationSessions : GenericRepository<MApplicationSessions>, IApplicationSessions
    {
        public ApplicationSessions(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<DTOActiveUserResponse> Concurrentuser(int ApplicationId)
        {

            var activeUsers = await _context.trnapplicationSessions
            .Where(s => s.SessionEndTime == null && s.ApplicationId == ApplicationId
                        && EF.Functions.DateDiffMinute(s.LastUpdated, DateTime.Now) <= 2
                        && s.IsActive == true)
            .GroupBy(s => s.ApplicationId)
            .Select(g => new DTOActiveUserResponse
            {
                ApplicationId = g.Key,
                Concurrentuser = g.Count()
            })
            .OrderBy(x => x.ApplicationId)
            .FirstOrDefaultAsync();

            if (activeUsers == null)
            {
                activeUsers = new DTOActiveUserResponse
                {
                    ApplicationId = ApplicationId,
                    Concurrentuser = 0
                };
            }
            return activeUsers;
        }

        public async Task<MApplicationSessionMirror> AddWithMirror(MApplicationSessionMirror data)
        {
            _context.trnApplicationSessionsMirror.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }


        public async Task<MApplicationSessions> GetApplicationBySessions(string ApplicationKey, string UserId)
        {

            var ret = await (from app in _context.trnapplicationDetails
                             join session in _context.trnapplicationSessions on app.ApplicationId equals session.ApplicationId
                             where app.ApplicationKey == ApplicationKey &&
                                   session.IpAddress == UserId &&
                                   session.IsActive == true && session.SessionStartTime.Date == DateTime.Now.Date
                             select new MApplicationSessions
                             {
                                 ApplicationSessionsId = session.ApplicationSessionsId,
                                 ApplicationId = app.ApplicationId,
                               
                                 SessionStartTime = session.SessionStartTime,
                                 SessionEndTime = session.SessionEndTime,
                                 IsActive = session.IsActive,
                                 LastUpdated = session.LastUpdated,
                                 IpAddress = session.IpAddress

                             }).FirstOrDefaultAsync();

            return ret;
        }

        public async Task<List<DTOConcurrentuserResponse>> Concurrentuser()
        {
            var activeUsers = await (
     from app in _context.trnapplicationDetails
     join sess in _context.trnapplicationSessions
         on app.ApplicationId equals sess.ApplicationId
     where sess.SessionEndTime == null
           && EF.Functions.DateDiffMinute(sess.LastUpdated, DateTime.Now) <= 4
           && sess.IsActive == true
     group sess by new { app.ApplicationId, app.ApplicationName } into g
     select new DTOConcurrentuserResponse
     {
         ApplicationId = g.Key.ApplicationId,
         ApplicationName = g.Key.ApplicationName,
         Total = g.Count()
     }
 )
 .OrderBy(x => x.ApplicationId)
 .ToListAsync();

            return activeUsers;

        }
        public async Task<List<DTOConcurrentuserResponse>> ConcurrentuserByUserId(int UserId)
        {
            var activeUsers = await (
     from app in _context.trnapplicationDetails
     join sess in _context.trnapplicationSessions
         on app.ApplicationId equals sess.ApplicationId
     where sess.SessionEndTime == null
           && EF.Functions.DateDiffMinute(sess.LastUpdated, DateTime.Now) <= 4
           && sess.IsActive == true && app.CreatedBy== UserId
     group sess by new { app.ApplicationId, app.ApplicationName } into g
     select new DTOConcurrentuserResponse
     {
         ApplicationId = g.Key.ApplicationId,
         ApplicationName = g.Key.ApplicationName,
         Total = g.Count()
     }
 )
 .OrderBy(x => x.ApplicationId)
 .ToListAsync();

            return activeUsers;

        }
        public async Task<List<DTOConcurrentuserResponse>> ConcurrentuserForDashboardByApplicationId(int ApplicationId)
        {
            var activeUsers = await (
     from app in _context.trnapplicationDetails
     join sess in _context.trnapplicationSessions
         on app.ApplicationId equals sess.ApplicationId
     where sess.SessionEndTime == null && sess.ApplicationId == ApplicationId
           && EF.Functions.DateDiffMinute(sess.LastUpdated, DateTime.Now) <= 4
           && sess.IsActive == true
     group sess by new { app.ApplicationId, app.ApplicationName } into g
     select new DTOConcurrentuserResponse
     {
         ApplicationId = g.Key.ApplicationId,
         ApplicationName = g.Key.ApplicationName,
         Total = g.Count()
     }
 )
 .OrderBy(x => x.ApplicationId)
 .ToListAsync();

            return activeUsers;

        }

        public async Task<List<MApplicationSessions>> ConcurrentuserList(int ApplicationId)
        {
            return await _context.trnapplicationSessions.Where(
                i => i.ApplicationId == ApplicationId
                && i.SessionEndTime == null
                   && EF.Functions.DateDiffMinute(i.LastUpdated, DateTime.Now) <= 4
                   && i.IsActive == true

            ).OrderByDescending(i => i.LastUpdated).ToListAsync();
        }

        public async Task<List<MApplicationHitsUserTrack>> ConcurrentuserListDatewise(int ApplicationId, DateTime date)
        {

            return await _context.trnApplicationHitsUserTrack.Where(
                i => i.ApplicationId == ApplicationId

                   && i.HitDate.Date == date.Date
            ).OrderByDescending(i => i.HitDate).ToListAsync();
        }
    }
}
