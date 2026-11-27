namespace study_room_booking.DTOs;

public class MyPageDto
{
    public bool HasStudentProfile { get; set; }
    public List<BookingDto> Bookings { get; set; } = new();
    public List<StudySessionDto> StudySessions { get; set; } = new();
}