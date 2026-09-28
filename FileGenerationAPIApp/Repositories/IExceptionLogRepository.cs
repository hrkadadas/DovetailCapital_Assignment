using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Repositories
{
    public interface IExceptionLogRepository
    {
        Task AddAsync(AppExceptionLog log);
        Task SaveChangesAsync();
    }
}
