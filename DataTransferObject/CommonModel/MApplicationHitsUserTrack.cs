using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.CommonModel
{
    public class MApplicationHitsUserTrack:Common
    {
        [Key]
        public int Id { get; set; }  // Primary key with auto-increment
        [Required]
        public int ApplicationId { get; set; }
      
        [Column(TypeName = "varchar(50)")]
        public string IpAddress { get; set; }  // User identifier (unique key)
        [Required]
        public DateTime HitDate { get; set; }  // The date and time when the hit was recorded

    }
}
