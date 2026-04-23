using SRp.Models;
using Microsoft.EntityFrameworkCore;

namespace SRp.Data
{
    public class PlayerInventoryContext : DbContext
    {
        public PlayerInventoryContext(DbContextOptions<PlayerInventoryContext> options)
            : base(options)
        { }

        public DbSet<PlayerInventory> PlayerInventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<PlayerInventory>()
            //    .HasMany(p => p.StackItems)
            //    .WithOne(s => s.PlayerInventory)
            //    .HasForeignKey(s => s.PlayerInventoryId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<SavedStackItem>()
            //    .Property(x => x.ItemId)
            //    .HasMaxLength(128)
            //    .IsRequired();

            //modelBuilder.Entity<SavedStackItem>()
            //    .HasIndex(x => new { x.PlayerInventoryId, x.ItemId })
            //    .IsUnique();
        }

    }
}
