using FileGenerationAPIApp.Data;
using FileGenerationAPIApp.Models;
using Microsoft.EntityFrameworkCore;
// Ensure EF Core namespace is imported; no-op if already present

namespace FileGenerationAPIApp.Repositories
{
    public class DesignationRepository : IDesignationRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public DesignationRepository(ApplicationDbContext context) => _applicationDbContext = context;

        public async Task AddAsync(Designation d) => await _applicationDbContext.Designations.AddAsync(d);

        public async Task<List<Designation>> GetAllAsync() => await _applicationDbContext.Designations.ToListAsync();

        public async Task SaveChangesAsync() => await _applicationDbContext.SaveChangesAsync();
    }
}
