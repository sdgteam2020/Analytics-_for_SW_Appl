using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Response
{
    public class DTOCounterHitsResponse
    {
        public int TotalHits { get; set; }
        public int TodayHits { get; set; }
        public int MonthlyHits { get; set; }
    }
}
