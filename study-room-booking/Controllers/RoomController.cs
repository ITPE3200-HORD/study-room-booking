// Here we define the controller for rooms, which handles requests from the browser
// and decides which view to show. All database access goes through the Room repository.

using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

public class RoomController : Controller
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<RoomController> _logger;

    public RoomController(IRoomRepository roomRepository, ILogger<RoomController> logger)
    {
        _roomRepository = roomRepository;
        _logger = logger;
    }

    // GET: /Room/Table
    // Shows all rooms in a table
    public async Task<IActionResult> Table()
    {
        var rooms = await _roomRepository.GetAll();
        if (rooms == null)
        {
            _logger.LogError("[RoomController] Room list not found while executing GetAll()");
            return NotFound("Room list not found");
        }
        var roomsViewModel = new RoomsViewModel(rooms.ToList(), "Table");
        return View(roomsViewModel);
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
            bool created = await _roomRepository.Create(room);
            if (created)
            {
                return RedirectToAction(nameof(Table)); // back to the room list
            }
        }
        _logger.LogWarning("[RoomController] Room creation failed {@room}", room);
        return View(room); // show the form again with errors
    }
}