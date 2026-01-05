using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.CommonModel
{
    public class MApplicationSessionMirror:Common
    {
        [Key]
        public int Id { get; set; }
        public int ApplicationSessionsId { get; set; }
        public int ApplicationId { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UserId { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string IpAddress { get; set; }  // User identifier (can be session ID or user ID)
        public DateTime SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }  // Nullable end time (if session has ended)
        public DateTime LastUpdated { get; set; }
        public DateTime MirrorOn { get; set; } // Equivalent to GetDate() in SQL
    }
}
