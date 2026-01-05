using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Response
{
    public class DTOConcurrentuserResponse
    {
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int Total { get; set; }
    }
}
