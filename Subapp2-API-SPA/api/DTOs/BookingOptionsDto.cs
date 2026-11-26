namespace study_room_booking.DTOs;

public class BookingOptionsDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsAvailable { get; set; }
}