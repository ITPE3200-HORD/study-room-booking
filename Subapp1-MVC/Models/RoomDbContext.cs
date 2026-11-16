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
    public DbSet<StudySession> StudySessions { get; set; } = null!;
    public DbSet<SessionParticipant> SessionParticipants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>()
            .HasOne(booking => booking.StudySession)
            .WithOne(studySession => studySession.Booking)
            .HasForeignKey<StudySession>(studySession => studySession.BookingId);
    }
}
