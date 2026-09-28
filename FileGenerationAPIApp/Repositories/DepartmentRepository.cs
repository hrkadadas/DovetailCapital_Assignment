using FileGenerationAPIApp.Data;
using FileGenerationAPIApp.Models;
using Microsoft.EntityFrameworkCore; // ensure EF Core using present

namespace FileGenerationAPIApp.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public DepartmentRepository(ApplicationDbContext context) => _applicationDbContext = context;

        public async Task AddAsync(Department d) => await _applicationDbContext.Departments.AddAsync(d);

        public async Task<List<Department>> GetAllAsync() => await _applicationDbContext.Departments.ToListAsync();

        public async Task SaveChangesAsync() => await _applicationDbContext.SaveChangesAsync();
    }
}
