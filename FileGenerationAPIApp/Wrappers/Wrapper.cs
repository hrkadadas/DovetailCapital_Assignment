using FileGenerationAPIApp.Repositories;

namespace FileGenerationAPIApp.Wrappers
{
    public class Wrapper : IWrapper
    {
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IDesignationRepository Designations { get; }
        public IDepartmentRepository Departments { get; }
        public IExceptionLogRepository ExceptionLogs { get; }

        public Wrapper(IUserRepository users, IRoleRepository roles, IDesignationRepository designations, IDepartmentRepository departments, IExceptionLogRepository exceptionLogs)
        {
            Users = users;
            Roles = roles;
            Designations = designations;
            Departments = departments;
            ExceptionLogs = exceptionLogs;
        }
    }
}
