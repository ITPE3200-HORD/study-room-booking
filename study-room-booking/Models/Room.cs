using System;
using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models
{
    public class Room
    {
        public int RoomId {get; set;}

        // Room name is required and must be 2-50 characters (letters, numbers, spaces, dots and dashes)
        [Required(ErrorMessage = "Room name is required")]
        [RegularExpression(@"^[a-zA-ZæøåÆØÅ0-9 .-]{2,50}$", ErrorMessage = "Room name must be 2-50 characters (letters, numbers, spaces, dots and dashes)")]
        public String RoomName {get; set;} = string.Empty;

        public String? Location {get; set;} //building/floot
        public int Capacity {get; set;}
        public bool IsActve {get; set;} //can the room be booked?
    }
}