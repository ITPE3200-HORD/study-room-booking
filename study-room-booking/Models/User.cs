using System;
namespace study_room_booking.Models;

public class User
{
    public int UserId {get; set;}
    public String FullName {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public bool IsActive {get; set;} = true;
}