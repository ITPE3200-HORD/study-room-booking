using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using study_room_booking.Models;

namespace study_room_booking.DAL;

public static class DbSeeder
{

    public static async Task SeedStudentAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = serviceProvider.GetRequiredService<RoomDbContext>();
        
        const string studentRole = "Student";

        var studentEmail = configuration["StudentUser:Email"];
        var studentPassword = configuration ["StudentUser:Password"];

    //Checks if the student exists - if not, create it
    if(string.IsNullOrWhiteSpace(studentEmail)|| string.IsNullOrWhiteSpace(studentPassword))
    {
        return;
    }

    if (!await roleManager.RoleExistsAsync(studentRole))
    await roleManager.CreateAsync(new IdentityRole(studentRole));

    var user = await userManager.FindByEmailAsync(studentEmail);

    if (user == null)
        {
            user = new IdentityUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, studentPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(exec_hook_info => exec_hook_info.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, studentRole))
        {
            var result = await userManager.AddToRoleAsync(user, studentRole);
            if (!result.Succeeded)
                throw new InvalidOperationException("Could not assign Student role.");
        }

    }
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = serviceProvider.GetRequiredService<RoomDbContext>();

        const string adminRole = "Admin";
        
        var adminEmail = configuration["AdminUser:Email"];
        var adminPassword = configuration["AdminUser:Password"];

       
        // Check if the Admin role exists, if not, create it
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            var result = await userManager.AddToRoleAsync(admin, adminRole);

            if (!result.Succeeded)
            throw new InvalidOperationException("Could not assign Admin role.");
        }
    }
    
    public static async Task SeedRoomsAsync(RoomDbContext db)
    {
        if (await db.Rooms.AnyAsync())
        {
            return; // Rooms already seeded
        }

        db.Rooms.AddRange(
            new Room { Location = "P35", RoomNumber = "101", RoomType = "Group Study Room", Capacity = 6, Status = Room.Availability.Available },
            new Room { Location = "B2", RoomNumber = "202", RoomType = "Class Room", Capacity = 30, Status = Room.Availability.Available },
            new Room { Location = "C3", RoomNumber = "303", RoomType = "Auditorium", Capacity = 120, Status = Room.Availability.NotAvailable }
        );

        await db.SaveChangesAsync();
    }

}