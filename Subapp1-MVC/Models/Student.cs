using Microsoft.AspNetCore.Identity;

namespace study_room_booking.Models;

public class Student
{
    public int StudentId { get; set; }
    public string IdentityUserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IdentityUser IdentityUser { get; set; } = null!; // Navigation property to the linked IdentityUser
    public virtual List<Booking> Bookings { get; set; } = new();
    public virtual List<SessionParticipant> SessionParticipants { get; set; } = new();
}
