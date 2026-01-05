using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface ICentralAnalyticsServiceSummary
    {
        public Task IncrementHitCounter(int ApplicationId);
    }
}
