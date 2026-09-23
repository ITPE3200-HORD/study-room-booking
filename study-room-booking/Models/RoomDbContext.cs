using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace study_room_booking.Models;

public class RoomDbContext : DbContext
{
    public RoomDbContext(DbContextOptions<RoomDbContext> options) : base (options)
    {
        Database.EnsureCreated();
    }

public DbSet<Room> Rooms {get; set;}
}