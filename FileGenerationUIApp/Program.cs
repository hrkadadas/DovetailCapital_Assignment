var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
// Configure a named HttpClient for calling backend APIs. Update ApiBaseUrl in appsettings.json if needed.
var apiBase = builder.Configuration["ApiBaseUrl"];
if (!string.IsNullOrEmpty(apiBase))
{
    builder.Services.AddHttpClient("ApiClient", client => client.BaseAddress = new Uri(apiBase));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
