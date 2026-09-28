using FileGenerationAPIApp.Models;
using FileGenerationAPIApp.Services;
using FileGenerationAPIApp.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileGenerationAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[Authorize(Roles = "Admin")]
    public class MastersController : ControllerBase
    {
        private readonly IWrapper _wrapper;
        private readonly ILogger<MastersController> _logger;
        private readonly IPasswordHasher _passwordHasher;

        public MastersController(IWrapper wrapper, ILogger<MastersController> logger, IPasswordHasher passwordHasher)
        {
            _wrapper = wrapper;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        // Users CRUD
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _wrapper.Users.GetAllAsync();
                return Ok(new ApiResponse { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUser createUser)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createUser.Password))
                    throw new ArgumentException("Password cannot be empty.");
                if (string.IsNullOrWhiteSpace(createUser.Username))
                    throw new ArgumentException("Username cannot be empty.");

                var user = new User
                {
                    Firstname = createUser.Firstname,
                    Lastname = createUser.Lastname,
                    Username = createUser.Username,
                    PasswordHash = _passwordHasher.HashPassword(createUser.Password),
                    RoleId = createUser.RoleId,
                    DesignationId = createUser.DesignationId,
                    DepartmentId = createUser.DepartmentId,                    
                };

                await _wrapper.Users.AddAsync(user);
                await _wrapper.Users.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new ApiResponse { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        // Roles
        [HttpGet("roles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _wrapper.Roles.GetAllAsync();
                return Ok(new ApiResponse { Success = true, Data = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            try
            {
                await _wrapper.Roles.AddAsync(role);
                await _wrapper.Roles.SaveChangesAsync();
                return CreatedAtAction(nameof(GetRoles), new { id = role.Id }, new ApiResponse { Success = true, Data = role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        // Designations
        [HttpGet("designations")]
        public async Task<IActionResult> GetDesignations()
        {
            try
            {
                var list = await _wrapper.Designations.GetAllAsync();
                return Ok(new ApiResponse { Success = true, Data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting designations");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("designations")]
        public async Task<IActionResult> CreateDesignation([FromBody] Designation d)
        {
            try
            {
                await _wrapper.Designations.AddAsync(d);
                await _wrapper.Designations.SaveChangesAsync();
                return CreatedAtAction(nameof(GetDesignations), new { id = d.Id }, new ApiResponse { Success = true, Data = d });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating designation");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        // Departments
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var list = await _wrapper.Departments.GetAllAsync();
                return Ok(new ApiResponse { Success = true, Data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("departments")]
        public async Task<IActionResult> CreateDepartment([FromBody] Department dep)
        {
            try
            {
                await _wrapper.Departments.AddAsync(dep);
                await _wrapper.Departments.SaveChangesAsync();
                return CreatedAtAction(nameof(GetDepartments), new { id = dep.Id }, new ApiResponse { Success = true, Data = dep });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }
    }
}
