using FileGenerationAPIApp.Filters;
using FileGenerationAPIApp.Repositories;
using FileGenerationAPIApp.Services;
using FileGenerationAPIApp.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// configure Serilog
var logDirectory = @"D:\Practices\Dovetail Capital\BackendAPI\log";

Directory.CreateDirectory(logDirectory);

var logFileName = $"log-{DateTime.Now:yyyyMMdd}.txt";

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File(
        Path.Combine(logDirectory, logFileName),
        shared: true)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Add authorization filter as a service so it can read token claims into HttpContext.Items
    options.Filters.AddService<TokenUserAuthorizationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // add JWT support to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement{
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme{
                Reference = new Microsoft.OpenApi.Models.OpenApiReference{ Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            }, new string[] { }
        }
    });
});

// PostgreSQL EF Core
var pgConn = builder.Configuration.GetConnectionString("PostgreSQL_ConnectionStrings");
if (string.IsNullOrEmpty(pgConn)) throw new InvalidOperationException("PostgreSQL_ConnectionStrings is not set in configuration.");
builder.Services.AddDbContext<FileGenerationAPIApp.Data.ApplicationDbContext>(opt =>
    opt.UseNpgsql(pgConn));

// register filters and services
builder.Services.AddScoped<TokenUserAuthorizationFilter>();
builder.Services.AddScoped<IDocumentGeneratorService, DocumentGeneratorService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();


// repositories and wrapper
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IExceptionLogRepository, ExceptionLogRepository>();
builder.Services.AddScoped<IWrapper, Wrapper>();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var issuer = jwtSection.GetValue<string>("Issuer");
var audience = jwtSection.GetValue<string>("Audience");

var key = new System.Text.UTF8Encoding().GetBytes(jwtKey ?? "dev_key");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// NOTE: Allow cross-origin requests for development UI -> API calls
app.UseCors("AllowLocalDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
