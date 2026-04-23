using SRp.Models;
using Microsoft.EntityFrameworkCore;

namespace SRp.Data
{
    public class SessionContext : DbContext
    {
        public SessionContext(DbContextOptions<SessionContext> options)
            : base(options)
        { }

        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Session>()
                .HasIndex(s => s.SessionId)
                .IsUnique();
        }

    }
}
