using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly RoomDbContext _roomDbContext;
    private readonly ILogger<HomeController> _logger;

    public HomeController(RoomDbContext roomDbContext, ILogger<HomeController> logger)
    {
        _roomDbContext = roomDbContext;
        _logger = logger;
    }

    // GET: /api/Home
    // First available room is displayed on the home page
    [HttpGet]
    public async Task<IActionResult> GetHomeData()
    {
        try
        {
            var rooms = await _roomDbContext.Rooms
                .AsNoTracking()
                .OrderBy(room => room.Status)
                .ThenBy(room => room.RoomNumber)
                .Select(room => new
                {
                    roomId = room.RoomId,
                    location = room.Location,
                    roomNumber = room.RoomNumber,
                    roomType = room.RoomType,
                    capacity = room.Capacity,
                    status = room.Status
                })
                .ToListAsync();

            var openStudySessions = await _roomDbContext.StudySessions
                .AsNoTracking()
                .Where(session =>
                    session.IsOpen &&
                    session.Booking.StartTime >= DateTime.Now &&
                    session.Participants.Count < session.MaxParticipants)
                .OrderBy(session => session.Booking.StartTime)
                .Select(session => new
                {
                    studySessionId = session.StudySessionId,
                    title = session.Title,
                    subject = session.Subject,
                    maxParticipants = session.MaxParticipants,
                    participantCount = session.Participants.Count,
                    roomId = session.Booking.RoomId,
                    roomNumber = session.Booking.Room.RoomNumber,
                    startTime = session.Booking.StartTime,
                    endTime = session.Booking.EndTime
                })
                .ToListAsync();

                return Ok(new
                {
                    rooms,
                    openStudySessions
                });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[HomeController] Failed to load home page data.");
            return Problem("Home page data could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // GET: 
    // /api/Home/available-rooms?date=2026-11-10&startTime=14:00:00&durationMinutes=120&groupSize=4
    [HttpGet("available-rooms")]
    public async Task<IActionResult> GetAvailableRooms(
        [FromQuery] DateTime date,
        [FromQuery] TimeSpan startTime,
        [FromQuery] int durationMinutes,
        [FromQuery] int groupSize,
        [FromQuery] string? roomType)
    {
        if (startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1) || durationMinutes is < 30 or > 240 || groupSize is < 1 or > 25)
        {
            return BadRequest("Date, time, duration or group size is invalid.");
        }

        var bookingStart = date.Date.Add(startTime);

        if (bookingStart <= DateTime.Now)
        {
            return BadRequest("The booking must start in the future.");
        }

        var bookingEnd = bookingStart.AddMinutes(durationMinutes);

        try
        {
            var availableRooms = await _roomDbContext.Rooms
                .AsNoTracking()
                .Where(room =>
                    room.Status == Room.Availability.Available &&
                    room.Capacity >= groupSize &&
                    // Room type filter: if no type is selected ("All room types"), all rooms pass.
                    // Otherwise, only rooms with the selected type are included.
                    (string.IsNullOrEmpty(roomType) || room.RoomType == roomType) &&
                    // Exclude rooms that have bookings overlapping with the requested time slot
                    !_roomDbContext.Bookings.Any(booking =>
                        booking.RoomId == room.RoomId &&
                        booking.StartTime < bookingEnd &&
                        bookingStart < booking.EndTime))
                .OrderBy(room => room.Capacity)
                .ThenBy(room => room.RoomNumber)
                .Select(room => new
                    {
                        roomId = room.RoomId,
                        location = room.Location,
                        roomNumber = room.RoomNumber,
                        roomType = room.RoomType,
                        capacity = room.Capacity
                    })
                    .ToListAsync();

                return Ok(new
                {
                    availableRoomCount = availableRooms.Count,
                    availableRooms
                });
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "[HomeController] Failed to search for available rooms.");

            return Problem("Available rooms could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
