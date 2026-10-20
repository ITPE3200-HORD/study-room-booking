using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace study_room_booking.Models;

public class RoomDbContext : IdentityDbContext
{
    public RoomDbContext(DbContextOptions<RoomDbContext> options) : base (options)
    {
       
    }

    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
}
