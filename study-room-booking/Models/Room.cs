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

        // Location is optional, but if filled in it must be 2-100 characters (letters, numbers, spaces and . , / -)
        [RegularExpression(@"^[a-zA-ZæøåÆØÅ0-9 .,/-]{2,100}$", ErrorMessage = "Location must be 2-100 characters (letters, numbers, spaces and . , / -)")]
        public String? Location {get; set;} //building/floor

        // Capacity must be between 1 and 50 people
        [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50")]
        public int Capacity {get; set;}

        public bool IsActve {get; set;} //can the room be booked?
    }
}