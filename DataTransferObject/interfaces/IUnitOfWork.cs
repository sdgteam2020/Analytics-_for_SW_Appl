using Domain.Requests;
using Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.interfaces
{
    public interface IUnitOfWork
    {
        IRank Rank { get; }
        public Task<List<DTOMasterResponse>> GetAllMMaster(DTOMasterRequest Data);
    }
}
