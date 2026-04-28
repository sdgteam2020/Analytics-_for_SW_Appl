using Domain.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Requests
{
    public class DTORequestApplicationDetails
    {
        [Key]
        public int ApplicationId { get; set; }
        [StringLength(50)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(@"^(?!\d+$)(?!\.+$)(?!_+$)[A-Za-z0-9._\s]+$", ErrorMessage = "Letters, numbers, spaces, . and _ are allowed, but the value cannot be only numbers, only dots, or only underscores.")]
        public required string ApplicationName { get; set; }
        [StringLength(50)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(@"^(?!\d+$)(?!\.+$)(?!_+$)[A-Za-z0-9._\s]+$", ErrorMessage = "Letters, numbers, spaces, . and _ are allowed, but the value cannot be only numbers, only dots, or only underscores.")]
        public required string Description { get; set; }
        [StringLength(50)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(
@"^(https?:\/\/)?(([\w-]+\.)+[\w-]+|(\d{1,3}\.){3}\d{1,3})(\/[\w\-._~:/?#[\]@!$&'()*+,;=%]*)?$",
ErrorMessage = "Please enter a valid URL")]
        public required string origin { get; set; }
    }
}
