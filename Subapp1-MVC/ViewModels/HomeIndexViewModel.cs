using System.Linq;
using study_room_booking.Models;

namespace study_room_booking.ViewModels;

public class HomeIndexViewModel
{
    public IReadOnlyList<Room> Rooms { get; set; } = 
        Array.Empty<Room>();

    public Room? FeaturedAvailableRoom =>
        Rooms.FirstOrDefault(room =>
            room.Status == Room.Availability.Available);

    public IReadOnlyList<StudySession> OpenStudySessions { get; set; } = 
        Array.Empty<StudySession>();
}