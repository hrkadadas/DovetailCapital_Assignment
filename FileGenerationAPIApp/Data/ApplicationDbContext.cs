using Microsoft.EntityFrameworkCore;
using FileGenerationAPIApp.Models;
using FileGenerationAPIApp.Configurations;

namespace FileGenerationAPIApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Designation> Designations => Set<Designation>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<AppExceptionLog> AppExceptionLogs => Set<AppExceptionLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configuration classes from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);            

            //modelBuilder.ApplyConfiguration(new RoleConfiguration());
            //modelBuilder.ApplyConfiguration(new DesignationConfiguration());
            //modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
            //modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
