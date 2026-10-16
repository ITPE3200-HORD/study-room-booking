using Microsoft.AspNetCore.Identity;

namespace study_room_booking.Models;

public class Student
{
    public int StudentId { get; set; }

    public string? IdentityUserId { get; set; }
    public IdentityUser? IdentityUser { get; set; }
}
