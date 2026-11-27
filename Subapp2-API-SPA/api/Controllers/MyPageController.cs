using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.DTOs;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MyPageController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly ILogger<MyPageController> _logger;

    public MyPageController(
        IStudentRepository studentRepository,
        IBookingRepository bookingRepository,
        IStudySessionRepository studySessionRepository,
        ILogger<MyPageController> logger)
    {
        _studentRepository = studentRepository;
        _bookingRepository = bookingRepository;
        _studySessionRepository = studySessionRepository;
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

        var student = await _studentRepository.GetByIdentityUserId(userId);

        // Only users with a student profile can have bookings and sessions (admins have none)
        if (student == null)
        {
            _logger.LogWarning("[MyPageController] No student profile found for user {UserId}", userId);
            return Ok(new MyPageDto {HasStudentProfile = false});
        }

        var bookings = await _bookingRepository.GetByStudentId(student.StudentId);
        var sessions = await _studySessionRepository.GetForStudent(student.StudentId);

        if (bookings == null || sessions == null)
        {
            _logger.LogError("[MyPageController] Failed to load data for student {StudentId}", student.StudentId);
            return Problem("My page data could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(new MyPageDto
        {
            HasStudentProfile = true,
            Bookings = bookings.Select(booking => booking.ToDto()).ToList(),
            StudySessions = sessions.Select(session => session.ToDto(student.StudentId)).ToList()
        });
    }
}