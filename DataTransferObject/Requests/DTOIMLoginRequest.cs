using Domain.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Requests
{
    public class DTOIMLoginRequest
    {
       
        [MaxLength(100, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
       
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}
