using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Requests
{
    public class DTOUserApprovalRequest
    {
        public int Id { get; set; }
        public bool Active { get; set; } = false;

    }
}
