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

    public MyPageController(RoomDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> MyPage()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var studentId = await _dbContext.Students
            .Where(student => student.IdentityUserId == userId)
            .Select(student => (int?)student.StudentId)
            .SingleOrDefaultAsync();

        var viewModel = new MyPageViewModel
        {
            HasStudentProfile = studentId.HasValue,
            Bookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(booking => booking.UserId == userId)
                .Include(booking => booking.Room)
                .OrderBy(booking => booking.StartTime)
                .ToListAsync()
        };

        if (studentId.HasValue)
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

        return View("~/Views/Account/MyPage.cshtml", viewModel);
    }
}