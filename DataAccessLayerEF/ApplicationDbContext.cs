using Domain.CommonModel;
using Domain.Identitytable;
using Domain.interfaces;
using Domain.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<MRank> MRank { get; set; } = null!;
        public DbSet<MApplicationDetails> trnapplicationDetails { get; set; }
        public DbSet<MApplicationSessions> trnapplicationSessions { get; set; }
        public DbSet<MApplicationSessionMirror> trnApplicationSessionsMirror { get; set; }
        public DbSet<MApplicationHits> trnmApplicationHits { get; set; }
        public DbSet<MApplicationHitsSummary> TrnMpplicationHitsSummary { get; set; }
        public DbSet<MApplicationHitsUserTrack> trnApplicationHitsUserTrack { get; set; }
        public DbSet<ExceptionLog> Log { get; set; }
    }
}
