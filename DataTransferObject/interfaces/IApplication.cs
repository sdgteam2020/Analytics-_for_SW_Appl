using Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface IApplication : IGenericRepository<MApplicationDetails>
    {
        Task<MApplicationDetails> GetApplicationByName(string ApplicationName);
        Task<MApplicationDetails> CheckApplicationKey(string ApplicationKey, string origin);
        Task<MApplicationDetails> CheckApplicationKey(string ApplicationKey);
        Task<List<MApplicationDetails>> GetApplication(int UserId);
    }
}
