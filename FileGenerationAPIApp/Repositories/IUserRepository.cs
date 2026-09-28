using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> FindByUsernameAndPasswordAsync(string username, string password);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
