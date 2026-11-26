namespace study_room_booking.DTOs;

// Information about the logged-in user, used by the frontend to show the right menus and pages
public class CurrentUserDto
{
    public string Username { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}