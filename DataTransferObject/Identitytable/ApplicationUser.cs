using Domain.Localize;
using Domain.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Identitytable
{
    public class ApplicationUser : IdentityUser<int>
    {


        [StringLength(10)]
        [Column(TypeName = "varchar(10)")]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(8, ErrorMessage = "Minimum length of Offr Army No is eight character.")]
        [MaxLength(10, ErrorMessage = "Maximum length of Offr Army No is ten character.")]
        [RegularExpression(@"^[\w]+$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "SpecialChars")]
        public string ArmyNo { get; set; } = string.Empty;

        [Required]
        [ForeignKey("MRank")]
        [RegularExpression(@"^[\d]+$", ErrorMessage = "RankId is number.")]
        public short RankId { get; set; }
        public MRank? MRank { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(@"^[\w ]+$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "SpecialChars")]
        public string Name { get; set; } = string.Empty;


        public bool Active { get; set; } = false;

        [Display(Name = "Updated By")]
        public int Updatedby { get; set; }

        [Display(Name = "Updated On")]
        [DataType(DataType.Date)]
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        public int RoleType { get; set; }
    }
}
