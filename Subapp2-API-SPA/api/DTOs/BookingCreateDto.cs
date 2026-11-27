using System.ComponentModel.DataAnnotations;

namespace study_room_booking.DTOs;

public class BookingCreateDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // open the booking as a study session in the same request
    public bool CreateStudySession { get; set; }

    [StringLength(100)]
    public string? SessionTitle { get; set; }
    
    [StringLength(100)]
    public string? SessionSubject { get; set; }

    [Range(2, 50, ErrorMessage = "Max participants must be between 2 and 50.")]
    public int? SessionMaxParticipants { get; set; }

    
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "End time must be after start time.",
                new[] { nameof(EndTime) });
        }

        if (StartTime <= DateTime.Now)
        {
            yield return new ValidationResult(
                "The booking must start in the future.",
                new[] { nameof(StartTime) });
        }

        if (CreateStudySession)
        {
            if (string.IsNullOrWhiteSpace(SessionTitle))
            {
                yield return new ValidationResult("Session title is required.", new [] { nameof(SessionTitle) });
            }

            if (SessionMaxParticipants == null)
            {
                yield return new ValidationResult("Max participants is required.", new [] { nameof(SessionMaxParticipants) });
            }
        }
    }
}