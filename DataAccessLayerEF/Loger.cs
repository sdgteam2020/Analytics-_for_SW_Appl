using Domain.Constants;
using Domain.interfaces;
using Domain.Model;
using Domain.Requests;
using Domain.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class Loger : GenericRepository<ExceptionLog>, ILoger
    {
        public Loger(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DTOGenericResponse<object>> AddAsync(DTOExceptionLogRequest Log)
        {
            ExceptionLog exceptionLog = new ExceptionLog();

            exceptionLog.OccurredAtUtc = Log.OccurredAtUtc;
            exceptionLog.CorrelationId = Log.CorrelationId;
            exceptionLog.Message = Log.Message?.Trim() ?? string.Empty;
            exceptionLog.StackTrace = Log.StackTrace;
            exceptionLog.Source = Log.Source;
            exceptionLog.Controller = Log.Controller;
            exceptionLog.Action = Log.Action;
            exceptionLog.Endpoint = Log.Endpoint;
            exceptionLog.HttpMethod = Log.HttpMethod;
            exceptionLog.Path = Log.Path;
            exceptionLog.QueryString = Log.QueryString;
            exceptionLog.SessionUser = Log.SessionUser;
            exceptionLog.Roles = Log.Roles;
            exceptionLog.RemoteIp = Log.RemoteIp;
            exceptionLog.UserAgent = Log.UserAgent;


            var rettrnmov = await AddWithReturn(exceptionLog);
            // Return success if both operations succeed
            return new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, rettrnmov.CorrelationId);
        }
    }
}
