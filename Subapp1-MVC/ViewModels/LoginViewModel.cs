using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models;
public class LoginViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    // Strongly typed property for the return URL after login
    public string? ReturnUrl { get; set; }

}