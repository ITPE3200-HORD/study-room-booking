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
}).AddEntityFrameworkStores<RoomDbContext>();

// Registers the Room repository so it can be injected into controllers
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();    
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
