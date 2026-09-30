using study_room_booking.DAL; // Room repository
using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RoomDbContext>(options =>

{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("RoomDbContextConnection"));
});

// Registers the Room repository so it can be injected into controllers
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();    
}

app.UseStaticFiles();

app.MapDefaultControllerRoute();

app.Run();
