using Domain.Constants;
using Domain.Identitytable;
using Domain.interfaces;
using Domain.Requests;
using Domain.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class Users : GenericRepository<ApplicationUser>, IUsers
    {
       

        public Users(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DTOUsersResponce>> GetAllData()
        {
            var getrole = await _context.Roles.ToListAsync();
            // Directly use _context.Users to query all users and include related tables (MRank and MDomain)
            var GetALL = await _context.Users
                .Include(A => A.MRank)       // Include the MRank related data
               
                
                .Select(A => new DTOUsersResponce
                {
                    Id = A.Id,
                    UserName = A.UserName,
                   
                    Name = A.Name,
                    Active = A.Active,
                    UpdatedOn = A.UpdatedOn,
                    RankName=A.MRank.RankAbbreviation
                })
                .ToListAsync();

            return GetALL;
        }

      
    }
}
