using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models
{
    public class Booking : IValidatableObject
    {
        public int BookingId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Velg et rom")]
        public int RoomId { get; set; }
        public virtual Room? Room { get; set; }

        public string? UserId { get; set; }

        [Required(ErrorMessage = "Navn må fylles ut")]
        [StringLength(50, ErrorMessage = "Navnet kan ikke være lengre enn 50 tegn")]
        public string? UserName { get; set; }

        [StringLength(200, ErrorMessage = "Beskrivelsen kan ikke være lengre enn 200 tegn")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Starttid må fylles ut")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Sluttid må fylles ut")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        // Egendefinert validering som går på tvers av flere felt
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
                yield return new ValidationResult("Sluttid må være etter starttid", new[] { nameof(EndTime) });

            if (StartTime < DateTime.Now)
                yield return new ValidationResult("Du kan ikke booke tilbake i tid", new[] { nameof(StartTime) });
        }
    }
}