// Inte// Interface for the Booking repository.
// Defines which database operations the BookingController can use.

using study_room_booking.Models;

namespace study_room_booking.DAL;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>?> GetAll();
    Task<Booking?> GetBookingById(int id);
    Task<bool> Create(Booking booking);
    Task<bool> Update(Booking booking);
    Task<bool> Delete(int id);
    Task<bool> IsOverlapping(Booking booking);
}