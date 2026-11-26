using System.ComponentModel.DataAnnotations;

namespace study_room_booking.DTOs;

public class StudySessionCreateDto
{
    [Range(1, int.MaxValue)]
    public int BookingId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subject { get; set; } = string.Empty;

    [Range(2, 50)]
    public int MaxParticipants { get; set; }
}