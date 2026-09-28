using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Repositories
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllAsync();
        Task AddAsync(Role role);
        Task SaveChangesAsync();
    }
}
