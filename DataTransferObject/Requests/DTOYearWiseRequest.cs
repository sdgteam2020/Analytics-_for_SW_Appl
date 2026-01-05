using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Requests
{
    public class DTOYearWiseRequest
    {
        public string ApplicationKey { get; set; }
        public int Years { get; set; }
    }
}
