using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

public class RoomController : Controller
{

    private readonly RoomDbContext _roomDbContext;

    public RoomController(RoomDbContext roomDbContext)
    {
        _roomDbContext = roomDbContext;
    }
    public IActionResult Table()
    {
        List<Room> rooms = _roomDbContext.Rooms.ToList();
        var roomsViewModel = new RoomsViewModel(rooms, "Table");
        return View(roomsViewModel);
    }

    public List<Room> GetRooms()
    {
        var rooms = new List<Room>();
        var room1 = new Room
        {
            RoomId = 1,
            RoomName = "Stainar",
            Location = "P35, 3. etg",
            Capacity = 5,
            IsActve = true
        };

        var room2 = new Room
        {
            RoomId = 2,
            RoomName = "Marianne",
            Location = "P32, 2. etg",
            Capacity = 4,
            IsActve = false
        };

        rooms.Add(room1);
        rooms.Add(room2);
        return rooms;
    }
}