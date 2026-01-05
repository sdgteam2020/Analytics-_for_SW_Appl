using Domain.interfaces;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class Application : GenericRepository<MApplicationDetails>, IApplication
    {
        public Application(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<MApplicationDetails> CheckApplicationKey(string ApplicationKey, string origin)
        {
            return await _context.trnapplicationDetails.Where(x => x.ApplicationKey == ApplicationKey && (x.origin == origin || x.origin == "*")).FirstOrDefaultAsync();
        }
        public async Task<MApplicationDetails> CheckApplicationKey(string ApplicationKey)
        {
            return await _context.trnapplicationDetails.Where(x => x.ApplicationKey == ApplicationKey).FirstOrDefaultAsync();
        }

        public async Task<List<MApplicationDetails>> GetApplication(int UserId)
        {
            return await _context.trnapplicationDetails.Where(x => x.CreatedBy == UserId).ToListAsync();
        }

        public async Task<MApplicationDetails> GetApplicationByName(string ApplicationName)
        {
            var allData = await _context.trnapplicationDetails
                .Where(i => i.ApplicationName == ApplicationName)
                .SingleOrDefaultAsync();

            return allData;
        }

    }
}
