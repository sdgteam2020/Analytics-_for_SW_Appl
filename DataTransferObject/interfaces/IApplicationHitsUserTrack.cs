using Domain.CommonModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface IApplicationHitsUserTrack : IGenericRepository<MApplicationHitsUserTrack>
    {
        public Task<bool> CheckHitIsToday(MApplicationHitsUserTrack Data);
    }
}
