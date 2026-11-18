using Microsoft.EntityFrameworkCore;
using study_room_booking.DAL;
using Scalar.AspNetCore;
using Serilog;
using study_room_booking.Models;
using Microsoft.AspNetCore.Identity;

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

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<RoomDbContext>();

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File($"Logs/api_{DateTime.Now:yyyyMMdd_HHmmss}.log");

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

// Create/update the database and add starting data when the app starts
// MigrateAsync applies any migrations that haven't run yet, so a fresh
// machine gets all tables without running "dotnet ef database update"
using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
    await db.Database.MigrateAsync();

    await DbSeeder.SeedAdminAsync(scope.ServiceProvider, builder.Configuration);
    await DbSeeder.SeedStudentAsync(scope.ServiceProvider, builder.Configuration);
    await DbSeeder.SeedRoomsAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();

app.Run();
