namespace study_room_booking.DTOs;

public class StudySessionDto
{
    public int StudySessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subject { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int ParticipantCount { get; set; }
    public bool IsOpen { get; set; }
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string OwnerUserName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool HasJoined { get; set; }

}