using System.ComponentModel.DataAnnotations;
namespace study_room_booking.ViewModels;
public class BookingCreationViewModel
{
    [Required]
    public int RoomId { get; set; }
    [Required]
    public string RoomNumber { get; set; } = "";
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
    [Required]
    public TimeSpan StartTime { get; set; }
    [Required]
    //Dropdown menu - no need for range
    public int DurationMinutes { get; set; }

    // If filled in by the user
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
    public string? Description { get; set; }
}