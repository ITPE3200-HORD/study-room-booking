using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.DTOs;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IRoomRepository roomRepository, IStudySessionRepository studySessionRepository, ILogger<HomeController> logger)
    {
        _roomRepository = roomRepository;
        _studySessionRepository = studySessionRepository;
        _logger = logger;
    }

    // GET: /api/Home
    // Returns all rooms and open study sessions with free spots for the home page
    [HttpGet]
    public async Task<IActionResult> GetHomeData()
    {
        var rooms = await _roomRepository.GetAll();
        var sessions = await _studySessionRepository.GetOpen(null, null);

        if (rooms == null || sessions == null)
        {
            _logger.LogError("[HomeController] Failed to load home page data.");
            return Problem ("Home page data could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var homeDto = new HomeDto
        {
            Rooms = rooms
                .OrderBy(room => room.Status)
                .ThenBy(room => room.RoomNumber)
                .Select(room => room.ToDto())
                .ToList(),
            // Only show sessions that still have free spots
            OpenStudySessions = sessions
                .Where(session => session.Participants.Count < session.MaxParticipants)
                .Select(session => session.ToDto(null))
                .ToList()
        };

        return Ok(homeDto);
    }

    // GET: /api/Home/available-rooms?date=2026-11-10&startTime=14:00:00&durationMinutes=120&groupSize=4
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

        var availableRooms = await _roomRepository.GetAvailable(bookingStart, bookingEnd, groupSize, roomType);
        
        if (availableRooms == null)
        {
            _logger.LogError("[HomeController] Failed to search for available rooms.");
            return Problem("Available rooms could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var roomDtos = availableRooms.Select(room => room.ToDto()).ToList();

        return Ok(new AvailableRoomsDto 
            {
                AvailableRoomCount = roomDtos.Count, AvailableRooms = roomDtos
                });
    }
}
