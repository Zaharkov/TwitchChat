using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain
{
    public class AppContext : DbContext
    {
        public AppContext() : base("Domain.AppContext")
        {
        }

        public DbSet<Log> Logs { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<ChatterInfo> ChattersInfo { get; set; }
    }
}
