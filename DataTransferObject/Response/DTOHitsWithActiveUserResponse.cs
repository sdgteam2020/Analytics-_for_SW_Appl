using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Response
{
    public class DTOHitsWithActiveUserResponse
    {

        public int TodayHits { get; set; }
        public int MonthlyHits { get; set; }
        public int TotalHits { get; set; }
        public int Concurrentuser { get; set; }
    }
}
