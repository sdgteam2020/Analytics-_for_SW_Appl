using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Response
{
    public class DTOHitForDgisResponse
    {
        public string Project { get; set; }
        public int Today { get; set; }
        public int CurrentMonth { get; set; }
        public int Concurrentuser { get; set; }
        public int Total { get; set; }
    }
}
