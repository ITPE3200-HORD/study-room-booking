using System;
namespace study_room_booking.Models
{
    public class Booking
    {
        public int BookinId { get; set;}
        public int RoomId {get; set;}
        public string? UserId {get; set;}
        public string? UserName {get; set;}
        public string? Description {get; set;}

        public DateTime StartTime {get; set;}
        public DateTime EndTTime {get; set;}
       
    }
}