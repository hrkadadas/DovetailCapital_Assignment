using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("role_tbl", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name").IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            //builder.Property<bool>("isactive").HasColumnName("isactive").HasDefaultValue(true);
        }
    }

    public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
    {
        public void Configure(EntityTypeBuilder<Designation> builder)
        {
            builder.ToTable("designation_tbl", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name").IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            //builder.Property<bool>("isactive").HasColumnName("isactive").HasDefaultValue(true);
        }
    }

    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("department_tbl", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name").IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            //builder.Property<bool>("isactive").HasColumnName("isactive").HasDefaultValue(true);
        }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("user_tbl", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Firstname).HasColumnName("firstname").IsRequired();
            builder.Property(x => x.Lastname).HasColumnName("lastname").IsRequired();
            builder.Property(x => x.Username).HasColumnName("username").IsRequired();
            builder.Property(x => x.PasswordHash).HasColumnName("passwordhash").IsRequired();
            builder.Property(x => x.RoleId).HasColumnName("roleid");
            builder.Property(x => x.DesignationId).HasColumnName("designationid");
            builder.Property(x => x.DepartmentId).HasColumnName("departmentid");
            builder.Property(x => x.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            //builder.Property<bool>("isactive").HasColumnName("isactive").HasDefaultValue(true);

            builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
            builder.HasOne(x => x.Designation).WithMany().HasForeignKey(x => x.DesignationId);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId);
        }
    }
    public class AppExceptionLogConfiguration : IEntityTypeConfiguration<AppExceptionLog>
    {
        public void Configure(EntityTypeBuilder<AppExceptionLog> builder)
        {
            builder.ToTable("appexceptionlog_tbl", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Message).HasColumnName("message").IsRequired();
            builder.Property(x => x.StackTrace).HasColumnName("stacktrace");
            builder.Property(x => x.Path).HasColumnName("path");
            builder.Property(x => x.Level).HasColumnName("level");
            builder.Property(x => x.UserId).HasColumnName("userid");
            builder.Property(x => x.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("now()");
        }
    }
}
