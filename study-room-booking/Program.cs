using study_room_booking.DAL; // Room repository
using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RoomDbContext>(options => {
    options.UseSqlite(
        builder.Configuration.GetConnectionString("RoomDbContextConnection"));
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<RoomDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Redirect to login page if not authenticated
    options.LogoutPath = "/Account/Logout"; // Redirect to logout page
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect to access denied page
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

// Registers the Room repository so it can be injected into controllers
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
    await db.Database.MigrateAsync();

    await DbSeeder.SeedAdminAsync(scope.ServiceProvider, builder.Configuration);
    await DbSeeder.SeedRoomsAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();    
}

app.UseStaticFiles();
app.UseSession(); // Enable session middleware

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
