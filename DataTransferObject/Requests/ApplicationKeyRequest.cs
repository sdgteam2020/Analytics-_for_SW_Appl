using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Requests
{
    public class ApplicationKeyRequest
    {
        public string ApplicationKey { get; set; }
        public int Years { get; set; }
        public int Month { get; set; }
    }
}
