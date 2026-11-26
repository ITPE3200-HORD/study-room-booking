using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models
{
    public class Booking : IValidatableObject
    {
        public int BookingId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a room")]
        public int RoomId { get; set; }
        public virtual Room? Room { get; set; }
        public virtual StudySession? StudySession { get; set; }

        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        // Custom validation across multiple fields
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
                yield return new ValidationResult("End time must be after start time", new[] { nameof(EndTime) });

            if (StartTime < DateTime.Now)
                yield return new ValidationResult("You cannot book in the past", new[] { nameof(StartTime) });
        }
    }
}