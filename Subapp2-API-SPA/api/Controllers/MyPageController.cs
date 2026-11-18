using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MyPageController : ControllerBase
{
    private readonly RoomDbContext _dbContext;
    private readonly ILogger<MyPageController> _logger;

    public MyPageController(RoomDbContext dbContext, ILogger<MyPageController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // GET: /api/MyPage
    [HttpGet]
    public async Task<IActionResult> GetMyPage()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        
        try
        {
            var studentId = await _dbContext.Students
                .AsNoTracking()
                .Where(student => student.IdentityUserId == userId)
                .Select(student => (int?)student.StudentId)
                .SingleOrDefaultAsync();
            
            // Admins (and others without a Student profile) have no study sessions
            var hasStudentProfile = studentId.HasValue;
            var currentStudentId = studentId ?? 0;


            var bookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(booking => booking.UserId == userId)
                .OrderBy(booking => booking.StartTime)
                .Select(booking => new
                {
                    bookingId = booking.BookingId,
                    roomId = booking.RoomId,
                    roomNumber = booking.Room == null
                        ? null
                        : booking.Room.RoomNumber,
                    booking.StartTime,
                    booking.EndTime,
                    booking.Description,
                    studySessionId = booking.StudySession == null
                        ? (int?)null
                        : booking.StudySession.StudySessionId
                })
                .ToListAsync();
            
            var studySessions = await _dbContext.StudySessions
                .AsNoTracking()
                .Where(session => session.Participants
                    .Any(participant => participant.StudentId == currentStudentId))
                .OrderBy(session => session.Booking.StartTime)
                .Select(session => new
                {
                    studySessionId = session.StudySessionId,
                    title = session.Title,
                    subject = session.Subject,
                    isOpen = session.IsOpen,
                    isOwner = session.Booking.UserId == userId,
                    maxParticipants = session.MaxParticipants,
                    participantCount = session.Participants.Count,
                    bookingId = session.BookingId,
                    roomId = session.Booking.RoomId,
                    roomNumber = session.Booking.Room == null ? null : session.Booking.Room.RoomNumber,
                    startTime = session.Booking.StartTime,
                    endTime = session.Booking.EndTime
                })
                .ToListAsync();

            return Ok(new
            {
                hasStudentProfile
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MyPageController] Failed to load data for user {UserId}.", userId);
            return Problem("My page data could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}