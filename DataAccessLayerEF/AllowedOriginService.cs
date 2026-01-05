using Domain.interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class AllowedOriginService : IAllowedOriginService
    {
        private HashSet<string> _allowedOrigins = new(StringComparer.OrdinalIgnoreCase);

        public AllowedOriginService(IServiceScopeFactory scopeFactory)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _allowedOrigins = db.trnapplicationDetails
                .Where(x => x.IsActive)
                .Select(x => x.origin)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public bool IsAllowed(string origin)
            => _allowedOrigins.Contains(origin);
    }

}
