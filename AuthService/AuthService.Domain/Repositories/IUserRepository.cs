using AuthService.Domain.Entities;

namespace AuthService.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task AddSync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}