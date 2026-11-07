using System.ComponentModel.DataAnnotations;

namespace study_room_booking.DTOs;

public class BookingUpdateDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

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
    }
}