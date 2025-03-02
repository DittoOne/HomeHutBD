using HomeHutBD.Data;
using HomeHutBD.Services;
using HomeHutBD.Models; // Ensure you have this if Users model is in Models namespace
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// Add Identity Services (This is missing in your code)
builder.Services.AddIdentity<Users, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication & Authorization Middleware
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Update with your actual login path
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add a hosted service (e.g., for a Flask API)
builder.Services.AddHostedService<FlaskServiceManager>();
builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Optionally wait for your hosted service to start (if needed)
    var flaskService = scope.ServiceProvider.GetRequiredService<IHostedService>() as FlaskServiceManager;
    await Task.Delay(5000);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Ensure Authentication & Authorization Middleware is Added
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
