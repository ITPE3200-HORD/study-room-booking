using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using study_room_booking.Models;
using study_room_booking.ViewModels;
using study_room_booking.DAL;


namespace study_room_booking.Controllers;

[Authorize]
public class MyPageController : Controller
{
    private readonly IStudentRepository _studentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<MyPageController> _logger;
    private readonly IStudySessionRepository _studySessionRepository;

    public MyPageController(IStudentRepository studentRepository, IBookingRepository bookingRepository, ILogger<MyPageController> logger, IStudySessionRepository studySessionRepository)
    {
        _studentRepository = studentRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
        _studySessionRepository = studySessionRepository;
    }

    // GET: /MyPage/MyPage
    [HttpGet]
    public async Task<IActionResult> MyPage()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var student = await _studentRepository.GetByIdentityUserId(userId);
            var bookings = await _bookingRepository.GetByUserId(userId);

            if (bookings == null)
            {
                _logger.LogError("Failed to fetch bookings for user {userId}.", userId);

                return StatusCode(500, "Internal server error");
            }

            var studySessions = student == null
                ? new List<StudySession>() : (await _studySessionRepository.GetForStudent(student.StudentId))?.ToList();

            if (studySessions == null)
            {
                _logger.LogError(
                    "Failed to fetch study sessions for StudentId {StudentId}.",
                    student!.StudentId);

                return StatusCode(500, "Internal server error");
            }

            var viewModel = new MyPageViewModel
            {
                HasStudentProfile = student != null,
                CurrentStudentId = student?.StudentId,
                Bookings = bookings.ToList(),
                StudySessions = studySessions
            };

            return View("~/Views/Account/MyPage.cshtml", viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load MyPage for userr {userId}.", userId);
            return StatusCode(500, "Internal serverr error");
        }
    }
}