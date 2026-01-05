using Domain.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Model
{
    public class MApplicationDetails : Common
    {
        [Key]
        [Required]
        public int ApplicationId { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]

        public string ApplicationName { get; set; }
        [Column(TypeName = "varchar(100)")]

        public string Description { get; set; }
        [Column(TypeName = "varchar(100)")]

        public string ApplicationKey { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string ColorCode { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string origin { get; set; }

    }
}
