using System.ComponentModel.DataAnnotations;

namespace study_room_booking.DTOs;

public class StudentUpdateDto
{
    public int StudentId { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;
}