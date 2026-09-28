using FileGenerationAPIApp.Data;
using FileGenerationAPIApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FileGenerationAPIApp.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public RoleRepository(ApplicationDbContext context) => _applicationDbContext = context;

        public async Task AddAsync(Role role) => await _applicationDbContext.Roles.AddAsync(role);

        public async Task<List<Role>> GetAllAsync() => await _applicationDbContext.Roles.ToListAsync();

        public async Task SaveChangesAsync() => await _applicationDbContext.SaveChangesAsync();
    }
}
