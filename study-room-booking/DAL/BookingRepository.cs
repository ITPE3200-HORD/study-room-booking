// Here we implement the Booking repository, 
// which handles all database access for bookings.
// Each method wraps the database call in try/catch, so errors are logged
// instead of crashing the application.

using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.DAL;

public class BookingRepository : IBookingRepository
{
    private readonly RoomDbContext _db;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(RoomDbContext db, ILogger<BookingRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Booking>?> GetAll()
    {
        try
        {
            // Include loads the related Room, so the room name can be shown in the table
            return await _db.Bookings.Include(b => b.Room).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[BookingRepository] GetAll() failed, error message: {e}", e.Message);
            return null;
        }
    }

    public async Task<Booking?> GetBookingById(int id)
    {
        try
        {
            return await _db.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.BookingId == id);
        }
        catch (Exception e)
        {
            _logger.LogError("[BookingRepository] GetBookingById() failed for BookingId {id}, error message: {e}", id, e.Message);
            return null;
        }
    }

    public async Task<bool> Create(Booking booking)
    {
        try
        {
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[BookingRepository] Create() failed for booking {@booking}, error message: {e}", booking, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(Booking booking)
    {
        try
        {
            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[BookingRepository] Update() failed for booking {@booking}, error message: {e}", booking, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null)
            {
                _logger.LogError("[BookingRepository] Booking not found for BookingId {id}", id);
                return false;
            }
            _db.Bookings.Remove(booking);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[BookingRepository] Delete() failed for BookingId {id}, error message: {e}", id, e.Message);
            return false;
        }
    }

    // Checks if the room is already booked in the given time period.
    // Excludes the booking itself, so updating a booking does not conflict with itself.
    public async Task<bool> IsOverlapping(Booking booking)
    {
        try
        {
            return await _db.Bookings.AnyAsync(b =>
                b.RoomId == booking.RoomId &&
                b.BookingId != booking.BookingId &&
                b.StartTime < booking.EndTime &&
                booking.StartTime < b.EndTime);
        }
        catch (Exception e)
        {
            _logger.LogError("[BookingRepository] IsOverlapping() failed for booking {@booking}, error message: {e}", booking, e.Message);
            // Treat errors as overlap, so we never accidentally double-book a room
            return true;
        }
    }
}