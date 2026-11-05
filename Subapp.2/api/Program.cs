using Microsoft.EntityFrameworkCore;
using study_room_booking.DAL;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using study_room_booking.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = // Prevents circular reference exceptions
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi(); //Native .Net OpenAPI support

builder.Services.AddDbContext<RoomDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("RoomDbContextConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy => 
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

builder.Services.AddScoped<IRoomRepository, RoomRepository>();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File($"Logs/api_{DateTime.Now:yyyyMMdd_HHmmss}.log");

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    DbSeeder.SeedRoomsAsync; // Seed the database with initial data
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.MapStaticAssets();
app.MapControllers();

app.Run();
