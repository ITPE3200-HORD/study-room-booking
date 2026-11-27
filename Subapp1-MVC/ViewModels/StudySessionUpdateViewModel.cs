using System.ComponentModel.DataAnnotations;

namespace study_room_booking.ViewModels;

public class StudySessionUpdateViewModel
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subject { get; set; }

    [Range(2, 50)]
    public int MaxParticipants { get; set; }

    public bool IsOpen { get; set; }

    public int StudySessionId { get; set; }
}