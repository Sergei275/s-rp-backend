using SRp.Models;
using Microsoft.EntityFrameworkCore;

namespace SRp.Data
{
    public class RefreshTokenContext : DbContext
    {
        public RefreshTokenContext(DbContextOptions<RefreshTokenContext> options)
            : base(options)
        { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>()
                .HasData();
        }

    }
}
