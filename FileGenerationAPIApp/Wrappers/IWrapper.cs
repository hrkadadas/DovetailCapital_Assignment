using FileGenerationAPIApp.Repositories;

namespace FileGenerationAPIApp.Wrappers
{
    public interface IWrapper
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IDesignationRepository Designations { get; }
        IDepartmentRepository Departments { get; }
        IExceptionLogRepository ExceptionLogs { get; }
    }
}
