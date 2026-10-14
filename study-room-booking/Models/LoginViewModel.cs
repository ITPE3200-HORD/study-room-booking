using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models;
public class LoginViewModel
{
    [Required]
    [Display(Name = "Brukernavn")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Passord")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

}