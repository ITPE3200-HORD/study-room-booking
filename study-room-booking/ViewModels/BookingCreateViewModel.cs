using System.ComponentModel.DataAnnotations;
namespace study_room_booking.ViewModels;

public class BookingCreateViewModel : IValidatableObject
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
    public int DurationMinutes { get; set; }

    // If filled in by the user
    [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters.")]
    public string? Description { get; set; }
    public bool CreateStudySession { get; set; }

    // If CreateStudySession is true, these fields will appear
    [StringLength(100, ErrorMessage = "Session title cannot be longer than 100 characters.")]
    public string? SessionTitle { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreateStudySession && string.IsNullOrWhiteSpace(SessionTitle))
        {
            yield return new ValidationResult(
                "Session title is required.",
                new[] { nameof(SessionTitle) });
        }
        // The duration must match the options on the search page (1-3 hours)
        // Checked on the server because the value is sent in the URL and can be changed.
        if (DurationMinutes < 60 || DurationMinutes > 180)
        {

            yield return new ValidationResult("The booking must last between 1 and 3 hours.");
        }
        // A booking cannot start in the past.
        // Date and StartTime are combined to get full start time.
        var start = Date.Date + StartTime;
        if (start < DateTime.Now)
        {
            yield return new ValidationResult("You cannot book in the past.");
        }
    }

    [StringLength(100, ErrorMessage = "Session subject cannot be longer than 100 characters.")]
    public string? SessionSubject { get; set; }

    [Range(1, 50, ErrorMessage = "Maximum participants must be between 1 and 50.")]
    public int SessionMaxParticipants { get; set; } = 2;
}
