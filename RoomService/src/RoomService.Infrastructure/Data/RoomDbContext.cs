using RoomService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace RoomService.Infrastructure.Data
{
    public class RoomDbContext : DbContext
    {
        public RoomDbContext(DbContextOptions<RoomDbContext> options)
            : base(options) { }

        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Equipment> Equipments { get; set; } = null!;
        //public DbSet<RoomEquipment> RoomEquipments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name).IsRequired();
                entity.Property(u => u.Number).IsRequired();

                entity.HasMany(x => x.Equipments)
                      .WithOne()
                      .HasForeignKey(x => x.RoomId)  
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Type).IsRequired();
                entity.Property(x => x.Brand).IsRequired();
                entity.Property(x => x.PurchaseDate).IsRequired();
            });

            modelBuilder.Entity<RoomEquipment>(entity =>
            {
                entity.HasKey(x => new { x.RoomId, x.EquipmentId });

                entity.HasIndex(x => x.EquipmentId)
                    .IsUnique();

                entity.HasOne<Equipment>()
                    .WithMany()
                    .HasForeignKey(x => x.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}