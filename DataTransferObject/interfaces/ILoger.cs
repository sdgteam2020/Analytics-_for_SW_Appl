using Domain.Model;
using Domain.Requests;
using Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface ILoger : IGenericRepository<ExceptionLog>
    {
        Task<DTOGenericResponse<object>> AddAsync(DTOExceptionLogRequest Log);
    }
}
