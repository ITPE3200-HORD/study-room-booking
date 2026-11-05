using Microsoft.AspNetCore.Identity;

namespace study_room_booking.DTOs;

public class Student
{
    public int StudentId { get; set; }
    public string IdentityUserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IdentityUser IdentityUser { get; set; } = null!; // Navigation property to the linked IdentityUser
}
