using System.ComponentModel.DataAnnotations;
 namespace study_room_booking.ViewModels;
public class BookingCreationViewModel
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = "";
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationMinutes { get; set; }


    // Fylles ut av brukeren
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}