using Microsoft.EntityFrameworkCore;

namespace Azumo.LANraragi.NET.Database
{
    public class LANraragiContext : DbContext
    {
        public LANraragiContext(DbContextOptions<LANraragiContext> options) : base(options)
        {
        }

        public DbSet<Archive> Archives { get; set; }
    }
}
