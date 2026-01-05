using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Response
{
    public class DTOAnalyticsSummary
    {
        public List<DTOApplicationMonthsWise> DTOApplicationMonthsWiseList { get; set; }
        public List<DTOApplicationTotalCount> DTOApplicationTotalCountList { get; set; }
        public List<DTOApplicationDayWise> DTOApplicationDayWiseList { get; set; }
    }
    public class DTOApplicationTotalCount
    {
        public string ApplicationName { get; set; }
        public int TotalHits { get; set; }
        public string ColorCode { get; set; }
    }
    public class DTOApplicationMonthsWise
    {
        public string ApplicationName { get; set; }
        public string MonthsName { get; set; }
        public int Years { get; set; }

        public int TotalHits { get; set; }
    }
    public class DTOApplicationDayWise
    {
        public string ApplicationName { get; set; }
        public string MonthsName { get; set; }
        public int Month { get; set; }
        public int Days { get; set; }

        public int TotalHits { get; set; }
    }
}
