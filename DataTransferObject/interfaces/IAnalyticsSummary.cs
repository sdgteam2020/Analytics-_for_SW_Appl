using Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface IAnalyticsSummary
    {
        Task<DTOAnalyticsSummary> GetDataSummary(int ApplicationId,int UserId, int Months, int Years);
        
    }
}
