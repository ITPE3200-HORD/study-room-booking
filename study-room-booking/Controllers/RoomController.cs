// Here we define the controller for rooms, which handles requests from the browser
// and decides which view to show. All database access goes through the Room repository.

using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

public class RoomController : Controller
{
    private readonly RoomDbContext _roomDbContext;
    private readonly ILogger<RoomController> _logger;

    public RoomController(RoomDbContext roomDbContext, ILogger<RoomController> logger)
    {
        _roomDbContext = roomDbContext;
        _logger = logger;
    }

    // GET: /Room/Table
    // Shows all rooms in a table
    public IActionResult Table()
    {
        List<Room> rooms = _roomDbContext.Rooms.ToList();
        var roomListViewModel = new RoomsViewModel(rooms, "Table");
        return View(roomListViewModel);
    }

    // GET: /Room/Create
    // Shows the empty form for creating a new room
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Room/Create
    // Saves the new room to the database if the form is valid
    [HttpPost]
    public async Task<IActionResult> Create(Room room)
    {
        if (ModelState.IsValid)
        {
            _roomDbContext.Rooms.Add(room);
            _roomDbContext.SaveChanges();

            // Log that the room was successfully created
            _logger.LogInformation("[RoomController] Room created: {RoomId} {RoomName}", room.RoomId, room.RoomName);
            return RedirectToAction(nameof(Table));
        }

        // Log a warning if the form input was invalid, so the error can be traced
        _logger.LogWarning("[RoomController] Room creation failed, ModelState invalid {RoomName}", room.RoomName);
        return View(room);   
    }
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var room = _roomDbContext.Rooms.Find(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    // POST: /Room/Update
    // Updates an existing room in the database if the form is valid
    [HttpPost]
    public async Task<IActionResult> Update (Room room)
    {
        if (ModelState.IsValid)
        {
            _roomDbContext.Rooms.Update(room);
            _roomDbContext.SaveChanges();

            // Log that the room was successfully updated
            _logger.LogInformation("[RoomController] Room updated: {RoomId} {RoomName}", room.RoomId, room.RoomName);
            return RedirectToAction(nameof(Table));
        }

        // Log a warning if the form input was invalid, so the error can be traced
        _logger.LogWarning("[RoomController] Room update failed, ModelState invalid {RoomName} {RoomId}", room.RoomName, room.RoomId);
        return View(room);
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var room = _roomDbContext.Rooms.Find(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
        var room = _roomDbContext.Rooms.Find(id);
        if (room == null)
        {
            return NotFound();
        }
        _roomDbContext.Rooms.Remove(room);
        _roomDbContext.SaveChanges();
        return RedirectToAction(nameof(Table));
    }
}