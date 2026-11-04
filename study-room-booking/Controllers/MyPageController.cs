using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using study_room_booking.ViewModels;
using study_room_booking.Models;

namespace study_room_booking.Controllers;

[Authorize]
public class MyPageController : Controller
{
    private readonly RoomDbContext _dbContext;
    private readonly ILogger<MyPageController> _logger;

    public MyPageController(RoomDbContext dbContext, ILogger<MyPageController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> MyPage()
    {
        int? studentId;
        List<Booking> bookings;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        
        try
        {
            studentId = await _dbContext.Students
                .Where(student => student.IdentityUserId == userId)
                .Select(student => (int?)student.StudentId)
                .SingleOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MyPageController] MyPage failed to fetch student profile for user ID {UserId}.", userId);
            return StatusCode(500, "Internal server error");
        }
        
        try
        {
            bookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(booking => booking.UserId == userId)
                .Include(booking => booking.Room)
                .OrderBy(booking => booking.StartTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MyPageController] MyPage failed to fetch bookings for user ID {UserId}.", userId);
            return StatusCode(500, "Internal server error");
        }

        var viewModel = new MyPageViewModel
        {
            HasStudentProfile = studentId.HasValue,
            Bookings = bookings
        };

        if (studentId.HasValue)
        {
            try
            {
                viewModel.StudySessions = await _dbContext.StudySessions
                    .AsNoTracking()
                    .Where(session => session.Participants
                        .Any(participant => participant.StudentId == studentId.Value))
                    .Include(session => session.Booking)
                        .ThenInclude(booking => booking.Room)
                    .OrderBy(session => session.Booking.StartTime)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[MyPageController] Failed to fetch study sessions for student ID {StudentId}.", studentId.Value);
                return StatusCode(500, "Internal server error");
            }
        }

        return View("~/Views/Account/MyPage.cshtml", viewModel);
    }
}