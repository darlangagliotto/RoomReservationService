using Microsoft.EntityFrameworkCore;

namespace ReservationService.Infrastructure.Data
{
    public class ReservationDbContext : DbContext
    {
        public ReservationDbContext(DbContextOptions<ReservationDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
