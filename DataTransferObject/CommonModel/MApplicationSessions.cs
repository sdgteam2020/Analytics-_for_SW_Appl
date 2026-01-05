using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.CommonModel
{
    public class MApplicationSessions:Common
    {
        [Key]
        public int ApplicationSessionsId { get; set; }  // Primary key with auto-increment

        public int ApplicationId { get; set; }  // Application identifier (unique key)
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UserId { get; set; }  // User identifier (can be session ID or user ID)
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string IpAddress { get; set; }  // User identifier (can be session ID or user ID)

        public DateTime SessionStartTime { get; set; }  // Session start time

        public DateTime? SessionEndTime { get; set; }  // Nullable end time (if session has ended)

    
        public DateTime LastUpdated { get; set; }  // Timestamp to track session updates
    }
}
