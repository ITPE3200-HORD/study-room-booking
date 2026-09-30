using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;

namespace study_room_booking.Controllers;

public class HomeController : Controller
{
    private readonly RoomDbContext _roomDbContext;

    public HomeController(RoomDbContext roomDbContext)
    {
        _roomDbContext = roomDbContext;
    }

    // First active room is displayed on the home page
    public IActionResult Index()
    {
        var room = _roomDbContext.Rooms.FirstOrDefault(room => room.IsActve);
        return View(room);
    }
}
