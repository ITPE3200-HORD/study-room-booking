using System.ComponentModel.DataAnnotations;

namespace study_room_booking.ViewModels;

public class StudentCreateViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set;} = string.Empty;
}