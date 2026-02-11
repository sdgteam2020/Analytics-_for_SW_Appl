using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Constants
{
    public class OptionalGuidAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var str = value.ToString();

            // Treat "0", empty, whitespace as NOT PROVIDED
            if (string.IsNullOrWhiteSpace(str) || str == "0")
                return ValidationResult.Success;

            return Guid.TryParse(str, out _)
                ? ValidationResult.Success
                : new ValidationResult("ApplicationKey must be a valid GUID");
        }
    }

}
