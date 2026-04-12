using ReservationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ReservationService.Infrastructure.Data
{
    public class ReservationDbContext : DbContext
    {
        public ReservationDbContext(DbContextOptions<ReservationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Reservation> Reservations {get; set;} = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>(entity =>
            {
               entity.HasKey(u => u.Id) ;
               entity.Property(u => u.UserId).IsRequired();
               entity.Property(u => u.RoomId).IsRequired();
               entity.Property(u => u.StartTime).IsRequired();
               entity.Property(u => u.EndTime).IsRequired();
            });
        }
    }
}
