using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.Controllers;

public class HomeController : Controller
{
    private readonly RoomDbContext _roomDbContext;

    public HomeController(RoomDbContext roomDbContext)
    {
        _roomDbContext = roomDbContext;
    }

    // First available room is displayed on the home page
    public IActionResult Index()
    {
        var room = _roomDbContext.Rooms.FirstOrDefault(r => r.Status == Room.Availability.Available);
        return View(room);
    }

    [HttpGet]
    public async Task<IActionResult> Book([FromQuery] BookViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var startTime = model.Date.ToDateTime(model.StartTime);

        if (startTime < DateTime.Now)
        {
            ModelState.AddModelError(nameof(BookViewModel.StartTime), "Choose a start time in the future.");
            return View(model);
        }

        var endTime = startTime.AddMinutes(model.DurationMinutes);

        model.AvailableRooms = await _roomDbContext.Rooms
            .AsNoTracking()
            .Where(room =>
                room.Status == Room.Availability.Available &&
                room.Capacity >= model.GroupSize &&
                !_roomDbContext.Bookings.Any(booking =>
                    booking.RoomId == room.RoomId &&
                    booking.StartTime < endTime &&
                    startTime < booking.EndTime))
            .OrderBy(room => room.Capacity)
            .ThenBy(room => room.RoomNumber)
            .ToListAsync();

        return View(model);
    }
}
