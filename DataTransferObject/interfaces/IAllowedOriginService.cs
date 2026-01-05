using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface IAllowedOriginService
    {
        bool IsAllowed(string origin);
    }
}
