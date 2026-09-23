using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;

namespace study_room_booking.Controllers;

public class RoomController : Controller
{
    public IActionResult Room()
    {
        var rooms = new List<Room>();

        var room1 = new Room
        {
            RoomId = 1,
            RoomName = "Steinar",
            Location = "Pilstredet 35, 3. etg",
            Capacity = 5
        };

        rooms.Add(room1);

        ViewBag.CurrenttViewName = "List of rooms";
        return View(rooms);
    }
}