using System.Data.Entity;

namespace Dominio
{
    public class DataProtectionKeysDbContext : DbContext
    {
        public DataProtectionKeysDbContext() : base("name=sharedcookieconnection")
        {
            Database.SetInitializer<DataProtectionKeysDbContext>(null);
        }

        public DbSet<DataProtectionKeyEF> DataProtectionKeys { get; set; }
    }
}