using CuttingOptimizer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<SheetMaterial> SheetMaterials { get; set; }
        public DbSet<EdgeBanding> EdgeBandings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SheetMaterial>(entity =>
            {
                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(s => s.Manufacturer)
                    .HasMaxLength(100);

                entity.Property(s => s.Length)
                    .HasPrecision(10, 2);

                entity.Property(s => s.Width)
                    .HasPrecision(10, 2);

                entity.Property(s => s.Thickness)
                    .HasPrecision(10, 2);
            });

            modelBuilder.Entity<EdgeBanding>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Thickness)
                    .HasPrecision(10, 2);
            });
        }
    }
}