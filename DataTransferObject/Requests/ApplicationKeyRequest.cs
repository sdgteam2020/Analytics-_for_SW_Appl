using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Requests
{
    public class ApplicationKeyRequest
    {
        [RegularExpression(
        @"^(\{)?[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[1-5][0-9a-fA-F]{3}\-[89abAB][0-9a-fA-F]{3}\-[0-9a-fA-F]{12}(\})?$",
        ErrorMessage = "ApplicationKey must be a valid GUID"
    )]
        public string ApplicationKey { get; set; }
        public int Years { get; set; }
        public int Month { get; set; }
    }
}
