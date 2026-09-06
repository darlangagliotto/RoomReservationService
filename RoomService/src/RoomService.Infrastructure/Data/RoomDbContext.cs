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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name).IsRequired();
                entity.Property(u => u.Number).IsRequired();
                entity.Property(u => u.PlanSlot);

                // Unicidade garantida pelo banco, e nao so pelo caso de uso —
                // ao contrario das demais unicidades do servico (backlog B5).
                // Filtrado para varias salas poderem ficar sem slot.
                entity.HasIndex(u => u.PlanSlot)
                      .IsUnique()
                      .HasFilter("\"PlanSlot\" IS NOT NULL");

                entity.HasMany(x => x.Equipments)
                      .WithOne()
                      .HasForeignKey(x => x.RoomId)  
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(x => x.Id);

                // Enum gravado como texto: legivel no banco e estavel a
                // reordenacao dos membros.
                entity.Property(x => x.Type)
                      .HasConversion<string>()
                      .HasMaxLength(40)
                      .IsRequired();

                entity.Property(x => x.Brand).IsRequired();
                entity.Property(x => x.PurchaseDate).IsRequired();

                // Placement e derivado do tipo, nao coluna.
                entity.Ignore(x => x.Placement);
            });

            modelBuilder.Entity<RoomEquipment>(entity =>
            {
                entity.HasKey(x => new { x.RoomId, x.EquipmentId });

                entity.HasIndex(x => x.EquipmentId)
                    .IsUnique();

                entity.Property(x => x.Placement)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.HasOne<Equipment>()
                    .WithMany()
                    .HasForeignKey(x => x.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
