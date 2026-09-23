using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RoomDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("RoomDbContextConnection"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();    
}

app.MapDefaultControllerRoute();

app.Run();
