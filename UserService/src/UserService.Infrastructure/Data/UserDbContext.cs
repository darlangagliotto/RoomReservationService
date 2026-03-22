using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name).IsRequired();
                entity.OwnsOne(u => u.Email, email =>
                {
                    email.Property(e => e.Value).HasColumnName("Email").IsRequired();
                });
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.IsBlocked).IsRequired();
            });
        }
    }
}