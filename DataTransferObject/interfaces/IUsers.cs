using Domain.Identitytable;
using Domain.Requests;
using Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface IUsers : IGenericRepository<ApplicationUser>
    {
        public Task<IEnumerable<DTOUsersResponce>> GetAllData();
      
    }
}
