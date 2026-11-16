using System;
using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models
{
    public class Room
    {
        public int RoomId {get; set;}

        // Location is not optional, and it must be 2-100 characters (letters, numbers, spaces and . , / -)
        [Required(ErrorMessage = "Location is required")]
        [RegularExpression(@"^[A-ZÆØÅ0-9]{2,5}$", 
            ErrorMessage = "Location must be 2-5 characters (Capital letters and numbers)")]
        public String? Location {get; set;} = string.Empty;//building

        [Required(ErrorMessage = "Room number is required")]
        [RegularExpression(@"^[A-ZÆØÅ0-9]{2,10}$", ErrorMessage = "Room number must be 2-10 characters (Capital letters,and numbers)")]
        public String RoomNumber {get; set;} = string.Empty;

        //Dont need regex due to drop-down menu
        public String RoomType {get; set;} = string.Empty;

        // Capacity must be between 1 and 50 people
        [Range(1, 250, ErrorMessage = "Capacity must be between 1 and 250")]
        public int Capacity {get; set;}

        public enum Availability
        {
            Available,
            NotAvailable
        }

        public Availability Status {get; set;} = Availability.Available;
    }
}