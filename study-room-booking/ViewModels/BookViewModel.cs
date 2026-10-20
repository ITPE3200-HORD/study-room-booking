using System.ComponentModel.DataAnnotations;

namespace study_room_booking.Models;
public class BookViewModel
{
    public Booking Booking { get; set; } = new();
    public int AvailableRoomCount { get; set; }
}