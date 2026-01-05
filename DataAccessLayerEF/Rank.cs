using Domain.interfaces;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class Rank : GenericRepository<MRank>, IRank
    {
        public Rank(ApplicationDbContext context) : base(context)
        {
        }
    }


}
