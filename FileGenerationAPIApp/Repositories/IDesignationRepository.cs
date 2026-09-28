using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Repositories
{
    public interface IDesignationRepository
    {
        Task<List<Designation>> GetAllAsync();
        Task AddAsync(Designation d);
        Task SaveChangesAsync();
    }
}
