using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.CommonModel
{
    public class MApplicationHitsSummary:Common
    {
        [Key]
        public int Id { get; set; }  // Primary key with auto-increment

        public int ApplicationId { get; set; }  // Application identifier (unique key)

        public DateTime Date { get; set; }  // The date for which the hit count is recorded
        public int TodayHits { get; set; }  // Number of hits for the current day

        //public int MonthHits { get; set; }  // Number of hits for the current month
    }
}
