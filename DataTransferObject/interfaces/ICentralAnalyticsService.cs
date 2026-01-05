using Domain.CommonModel;
using Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface ICentralAnalyticsService
    {
        public Task IncrementHitCounter(int ApplicationId);
        public Task<DTOCounterHitsResponse> HitCounter(int ApplicationId);
        public Task<List<DTOHitForDgisResponse>> HitCounter();
        public Task<MApplicationHits> GetApplicationHits(int ApplicationId);
        public Task<int> GetTotalHitsForApp(int ApplicationId);
        public Task<int> GetTodayHitsForApp(int ApplicationId);
        public Task<int> GetMonthHitsForApp(int ApplicationId);
    }
}
