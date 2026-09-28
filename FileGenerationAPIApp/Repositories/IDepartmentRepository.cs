using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Repositories
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task AddAsync(Department d);
        Task SaveChangesAsync();
    }
}
