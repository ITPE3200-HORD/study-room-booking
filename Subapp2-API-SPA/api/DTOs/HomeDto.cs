namespace study_room_booking.DTOs;

public class HomeDto
{
    public List<RoomDto> Rooms { get; set; } = new();
    public List<StudySessionDto> OpenStudySessions { get; set; } = new();
}