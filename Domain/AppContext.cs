using System.Data.Entity;
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
        public DbSet<RouletteInfo> RoulettesInfo { get; set; }
    }
}
