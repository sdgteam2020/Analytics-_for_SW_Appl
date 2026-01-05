using Domain.CommonModel;
using Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface IApplicationSessions : IGenericRepository<MApplicationSessions>
    {

        Task<MApplicationSessions> GetApplicationBySessions(string ApplicationKey, string UserId);
        Task<MApplicationSessionMirror> AddWithMirror(MApplicationSessionMirror data);
        Task<DTOActiveUserResponse> Concurrentuser(int ApplicationId);
        Task<List<DTOConcurrentuserResponse>> Concurrentuser();
        Task<List<DTOConcurrentuserResponse>> ConcurrentuserByUserId(int UserId);
        Task<List<DTOConcurrentuserResponse>> ConcurrentuserForDashboardByApplicationId(int ApplicationId);

        Task<List<MApplicationSessions>> ConcurrentuserList(int ApplicationId);
        Task<List<MApplicationHitsUserTrack>> ConcurrentuserListDatewise(int ApplicationId, DateTime date);
    }
}
