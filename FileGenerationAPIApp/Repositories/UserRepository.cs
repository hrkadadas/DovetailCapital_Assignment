using FileGenerationAPIApp.Data;
using FileGenerationAPIApp.Models;
using FileGenerationAPIApp.Services;
using Microsoft.EntityFrameworkCore;

namespace FileGenerationAPIApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IPasswordHasher _passwordHasher;
        public UserRepository(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _applicationDbContext = context;
            _passwordHasher = passwordHasher;
        }

        public async Task AddAsync(User user)
        {
            await _applicationDbContext.Users.AddAsync(user);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _applicationDbContext.Users.Include(u => u.Role).Include(u => u.Designation).Include(u => u.Department).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id) => await _applicationDbContext.Users.FindAsync(id);

        public async Task<User?> FindByUsernameAndPasswordAsync(string username, string password)
        {
            var user = await _applicationDbContext.Users
                        .Include(u => u.Role)
                        .Include(u => u.Designation)
                        .Include(u => u.Department)
                        .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task SaveChangesAsync() => await _applicationDbContext.SaveChangesAsync();
    }
}
