using System.ComponentModel.DataAnnotations;
namespace study_room_booking.DTOs {
    public class StudySession
    {
    public int StudySessionId { get; set; } // PK

    public int BookingId { get; set; } // FK to Booking
    public virtual Booking Booking { get; set; } = default!; // Navigation property to Booking

    [Required, StringLength(100)]
    public string Title {get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subject { get; set; }

    [Range(1, 50)]
    public int MaxParticipants { get; set; }

    public bool IsOpen { get; set; } = true; // Indicates if the session is open for new participants

    public virtual List<SessionParticipant> Participants { get; set; } = new();

    }
 }

