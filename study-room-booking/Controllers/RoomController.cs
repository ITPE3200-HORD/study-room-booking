using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;

namespace study_room_booking.Controllers;

public class RoomController : Controller
{
    private readonly RoomDbContext _roomDbContext;
    
    public RoomController(RoomDbContext roomDbContext)
    {
        _roomDbContext = roomDbContext;
    }
       public IActionResult Room()
    {
        List<Room> rooms = _roomDbContext.Rooms.ToList();
        return View(RoomViewModel);
}