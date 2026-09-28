using FileGenerationAPIApp.Data;
using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Repositories
{
    public class ExceptionLogRepository : IExceptionLogRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ExceptionLogRepository(ApplicationDbContext context) => _applicationDbContext = context;

        public async Task AddAsync(AppExceptionLog log) => await _applicationDbContext.AppExceptionLogs.AddAsync(log);

        public async Task SaveChangesAsync() => await _applicationDbContext.SaveChangesAsync();
    }
}
