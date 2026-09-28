namespace FileGenerationAPIApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string Username { get; set; } = null!;
        // For prototype only; store hashed passwords in real apps
        public string PasswordHash { get; set; } = null!;
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public int? DesignationId { get; set; }
        public Designation? Designation { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

public class CreateUser
{
    //public int Id { get; set; }
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int RoleId { get; set; }
    public int? DesignationId { get; set; }
    public int? DepartmentId { get; set; }
    //public bool IsActive { get; set; } = true;
}
