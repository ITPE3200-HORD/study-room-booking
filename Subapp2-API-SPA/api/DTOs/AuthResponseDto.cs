namespace study_room_booking.DTOs;

// Returned after a successful register or login
public class AuthResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}