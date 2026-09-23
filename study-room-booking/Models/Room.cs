using System;
namespace study_room_booking.Models
{
    public class study_room_booking
    {
        public int RoomId {get; set;}
        public String RoomName {get; set;} = string.Empty;
        public String? Location {get; set;} //building/floot
        public int Capacity {get; set;}
        public bool IsActve {get; set;} //can tthe room be booked?
    }
}