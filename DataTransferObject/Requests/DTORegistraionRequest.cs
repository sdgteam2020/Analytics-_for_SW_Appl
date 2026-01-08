using Domain.Localize;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Requests
{
    public class DTORegistraionRequest
    {
        [StringLength(20)]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(20, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(
    @"^[a-zA-Z0-9]+$",
    ErrorMessageResourceType = typeof(ErrorMessages),
    ErrorMessageResourceName = "AlphaNumericOnly"
)]

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [ForeignKey("MRank")]
        [RegularExpression(@"^[\d]+$", ErrorMessage = "RankId is number.")]
        public short RankId { get; set; }
        public MRank? MRank { get; set; }

        [StringLength(50)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(@"^[a-zA-Z]+( [a-zA-Z]+)*$",ErrorMessageResourceType = typeof(ErrorMessages),ErrorMessageResourceName = "Alphawithspace")]

        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(
    @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
    ErrorMessageResourceType = typeof(ErrorMessages),
    ErrorMessageResourceName = "StrongPassword"
)]
        public string Password { get; set; } = string.Empty;


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
