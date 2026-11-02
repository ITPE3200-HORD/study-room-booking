using study_room_booking.Models;

namespace study_room_booking.ViewModels;

public class MyPageViewModel
{
    public List<Booking> Bookings { get; set; } = new();
    public List<StudySession> StudySessions { get; set; } = new();
    public bool HasStudentProfile { get; set; }
}