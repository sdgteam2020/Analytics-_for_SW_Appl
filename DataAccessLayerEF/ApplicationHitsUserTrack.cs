using Domain.CommonModel;
using Domain.interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class ApplicationHitsUserTrack : GenericRepository<MApplicationHitsUserTrack>, IApplicationHitsUserTrack
    {
        public ApplicationHitsUserTrack(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> CheckHitIsToday(MApplicationHitsUserTrack Data)
        {

            // Check if a hit for the same user and application exists today
            var ret = await _context.trnApplicationHitsUserTrack
                       .AnyAsync(hit => hit.UserId == Data.UserId &&
                        hit.ApplicationId == Data.ApplicationId &&
                        hit.HitDate.Date == Data.HitDate.Date);
            return ret;
        }
    }
}
