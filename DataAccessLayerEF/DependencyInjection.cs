using Domain.interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace DataAccessLayerEF
{
    public static class DependencyInjection
    {


        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IRank, Rank>();

            services.AddTransient<IApplication, Application>();
            services.AddTransient<IApplicationSessions, ApplicationSessions>();
            services.AddTransient<ICentralAnalyticsService, CentralAnalyticsService>();
            services.AddTransient<ICentralAnalyticsServiceSummary, CentralAnalyticsServiceSummary>();
            services.AddTransient<IApplicationHitsUserTrack, ApplicationHitsUserTrack>();
            services.AddTransient<IAnalyticsSummary, AnalyticsSummary>();
            services.AddTransient<IUsers, Users>();
            services.AddTransient<IAllowedOriginService, AllowedOriginService>();
            services.AddTransient<ILoger, Loger>();


            return services;
        }
    }
}
