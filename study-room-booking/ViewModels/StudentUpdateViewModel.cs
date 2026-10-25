using System.ComponentModel.DataAnnotations;

namespace study_room_booking.ViewModels;

public class StudentUpdateViewModel
{
    public int StudentId { get; set; }
    
    [Required]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(256)]
    public string Username { get; set; } = string.Empty;
}