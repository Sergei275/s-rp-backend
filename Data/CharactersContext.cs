using SRp.Models;
using Microsoft.EntityFrameworkCore;

namespace SRp.Data
{
    public class CharactersContext : DbContext
    {
        public CharactersContext(DbContextOptions<CharactersContext> options)
            : base(options)
        { }

        public DbSet<Character> Characters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .ToTable("Players");

            modelBuilder.Entity<Character>(entity =>
            {
                entity.ToTable("Characters");

                entity.HasIndex(character => new
                {
                    character.PlayerId,
                    character.CharacterId
                })
                .IsUnique();
            });
        }
    }
}
