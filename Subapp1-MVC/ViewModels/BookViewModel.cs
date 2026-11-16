using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models;

public class BookViewModel
{
    [DataType(DataType.Date)]
    public DateOnly Date { get; set; } =
        DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; } =
        new(14, 0);

    [Range(30, 240, ErrorMessage = "Choose a duration between 30 minutes and 4 hours.")]
    public int DurationMinutes { get; set; } = 120;

    [Range(1, 25, ErrorMessage = "Group size must be between 1 and 25.")]
    public int GroupSize { get; set; } = 4;
    
    public string? RoomType { get; set; }

    public IReadOnlyList<Room> AvailableRooms { get; set; } =
        Array.Empty<Room>();

    public int AvailableRoomCount => AvailableRooms.Count;
}
