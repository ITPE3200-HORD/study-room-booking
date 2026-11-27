namespace study_room_booking.DTOs;

public class BookingDto
{
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int? StudySessionId { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}